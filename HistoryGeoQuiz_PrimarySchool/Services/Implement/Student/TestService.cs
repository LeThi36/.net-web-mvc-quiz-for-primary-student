using HistoryGeoQuiz_PrimarySchool.Models;
using HistoryGeoQuiz_PrimarySchool.ViewModels;
using HistoryGeoQuiz_PrimarySchool.ViewModels.Student;
using HistoryGeoQuiz_PrimarySchool.Repositories.Interfaces;
using HistoryGeoQuiz_PrimarySchool.Services.Interfaces;
using HistoryGeoQuiz_PrimarySchool.Helpers;

namespace HistoryGeoQuiz_PrimarySchool.Services.Implement.Student
{
    /// <summary>
    /// Test/quiz service. Contains business logic for quiz taking, scoring, and results.
    /// All DB queries go through repositories.
    /// </summary>
    public class TestService : ITestService
    {
        private readonly IUserRepository _userRepo;
        private readonly ILessonRepository _lessonRepo;
        private readonly ITestResultRepository _testResultRepo;
        private readonly ILogger<TestService> _logger;

        public TestService(
            IUserRepository userRepo,
            ILessonRepository lessonRepo,
            ITestResultRepository testResultRepo,
            ILogger<TestService> logger)
        {
            _userRepo = userRepo;
            _lessonRepo = lessonRepo;
            _testResultRepo = testResultRepo;
            _logger = logger;
        }

        public async Task<(User? Student, string? ClassName, string? HomeroomTeacher)> GetStudentInfoAsync(Guid userId)
        {
            var student = await _userRepo.GetStudentWithClassInfoAsync(userId);
            return (student, student?.ClassRoom?.ClassName, student?.ClassRoom?.HomeroomTeacher?.FullName);
        }

        public async Task<StudentDashboardViewModel> GetStudentDashboardAsync(Guid userId)
        {
            // Get student basic info including class and homeroom teacher
            var (_, className, teacherName) = await GetStudentInfoAsync(userId);

            var student = await _userRepo.GetByIdAsync(userId);
            var classRoomId = student?.ClassRoomId;

            // Get active lessons for this class
            var lessons = classRoomId.HasValue
                ? await _lessonRepo.GetActiveByClassAsync(classRoomId.Value)
                : new List<Lesson>();

            // Get student's test results
            var testResults = await _testResultRepo.GetByStudentAsync(userId);

            // Business logic: build lesson cards with attempt info
            var lessonCards = new List<LessonCardViewModel>();
            foreach (var lesson in lessons)
            {
                if (lesson.Questions.Count == 0) continue;

                var studentResults = testResults.Where(tr => tr.LessonId == lesson.Id).ToList();
                lessonCards.Add(new LessonCardViewModel
                {
                    Id = lesson.Id,
                    Title = lesson.Title,
                    Description = lesson.Description,
                    Subject = lesson.Subject,
                    LessonNumber = lesson.LessonNumber,
                    QuestionCount = lesson.Questions.Count,
                    HasAttempted = studentResults.Any(),
                    BestScore = studentResults.Any() ? studentResults.Max(r => r.Score) : null
                });
            }

            // Get recent results
            var recentResults = await _testResultRepo.GetRecentByStudentAsync(userId, 5);

            return new StudentDashboardViewModel
            {
                ClassName = className,
                HomeroomTeacherName = teacherName,
                Lessons = lessonCards,
                RecentResults = recentResults
            };
        }

        public async Task<TakeQuizViewModel?> GetQuizAsync(Guid lessonId, Guid userId)
        {
            var student = await _userRepo.GetByIdAsync(userId);
            var lesson = await _lessonRepo.GetActiveWithQuestionsAsync(lessonId);

            // Business rule: student must belong to the lesson's class
            if (lesson == null || lesson.ClassRoomId != student?.ClassRoomId)
                return null;

            if (!lesson.Questions.Any())
                return null;

            return new TakeQuizViewModel
            {
                LessonId = lesson.Id,
                LessonTitle = lesson.Title,
                Subject = lesson.Subject,
                TimeLimitMinutes = lesson.TimeLimitMinutes,
                Questions = lesson.Questions
                    .OrderBy(q => q.OrderIndex)
                    .Select((q, index) => new QuizQuestionViewModel
                    {
                        QuestionId = q.Id,
                        QuestionNumber = index + 1,
                        QuestionText = q.QuestionText,
                        Answers = q.Answers
                            .OrderBy(a => a.AnswerLabel)
                            .Select(a => new QuizAnswerViewModel
                            {
                                AnswerId = a.Id,
                                AnswerLabel = a.AnswerLabel,
                                AnswerText = a.AnswerText
                            }).ToList()
                    }).ToList()
            };
        }

        public async Task<TestResult> SubmitQuizAsync(Guid lessonId, Dictionary<string, Guid> answers, Guid userId, int timeTakenSeconds = 0)
        {
            var student = await _userRepo.GetByIdAsync(userId);
            var lesson = await _lessonRepo.GetActiveWithQuestionsAsync(lessonId);
            
            if (lesson == null)
                throw new ArgumentException("Lesson not found");

            if (lesson.ClassRoomId != student?.ClassRoomId)
                throw new UnauthorizedAccessException("Bạn không có quyền nộp bài cho lớp học này.");

            // Business logic: parse answer keys
            var answerDict = answers.ToDictionary(
                kvp => Guid.Parse(kvp.Key.Replace("answers[", "").Replace("]", "")),
                kvp => kvp.Value
            );

            // Business logic: scoring
            int correctCount = 0;
            var testDetails = new List<TestDetail>();

            foreach (var question in lesson.Questions)
            {
                var selectedAnswerId = answerDict.ContainsKey(question.Id) ? answerDict[question.Id] : (Guid?)null;
                var correctAnswer = question.Answers.FirstOrDefault(a => a.IsCorrect);
                var isCorrect = selectedAnswerId != null && correctAnswer != null && selectedAnswerId == correctAnswer.Id;

                if (isCorrect) correctCount++;

                testDetails.Add(new TestDetail
                {
                    QuestionId = question.Id,
                    SelectedAnswerId = selectedAnswerId,
                    IsCorrect = isCorrect
                });
            }

            int totalQuestions = lesson.Questions.Count;
            double score = totalQuestions > 0 ? Math.Round((double)correctCount / totalQuestions * 10, 1) : 0;

            var testResult = new TestResult
            {
                StudentId = userId,
                LessonId = lessonId,
                TotalQuestions = totalQuestions,
                CorrectAnswers = correctCount,
                Score = score,
                IsPassed = score >= 5,
                CompletedAt = DateTimeHelper.GetVietnamTime(),
                TimeTakenSeconds = timeTakenSeconds,
                // P1.3 FIX: Use navigation property — EF Core auto-assigns FK
                Details = testDetails
            };

            await _testResultRepo.AddAsync(testResult);
            await _testResultRepo.SaveChangesAsync(); // 1 round-trip instead of 2

            _logger.LogInformation("Quiz submitted: Lesson={LessonId}, Student={UserId}, Score={Score}",
                lessonId, userId, score);
            return testResult;
        }

        public async Task<QuizResultViewModel?> GetResultAsync(Guid testResultId, Guid userId)
        {
            var result = await _testResultRepo.GetWithLessonAsync(testResultId, userId);
            if (result == null) return null;

            // Business logic: grading thresholds
            int excellentThreshold = result.Lesson?.QuestionCountForExcellent ?? (int)Math.Ceiling(result.TotalQuestions * 0.9);
            int goodThreshold = result.Lesson?.QuestionCountForGood ?? (int)Math.Ceiling(result.TotalQuestions * 0.7);

            // Business logic: grading messages
            string message;
            if (result.CorrectAnswers >= excellentThreshold)
                message = "🌟 Hoàn thành xuất sắc! Con giỏi lắm! 🌟";
            else if (result.CorrectAnswers >= goodThreshold)
                message = "👍 Hoàn thành tốt! Con làm tốt lắm! 👍";
            else if (result.Score >= 5)
                message = "😊 Hoàn thành! Cố gắng thêm nhé! 😊";
            else
                message = "💪 Chưa đạt! Con ôn bài và làm lại nhé! 💪";

            return new QuizResultViewModel
            {
                TestResultId = result.Id,
                LessonTitle = result.Lesson?.Title ?? "",
                Subject = result.Lesson?.Subject ?? "",
                TotalQuestions = result.TotalQuestions,
                CorrectAnswers = result.CorrectAnswers,
                Score = result.Score,
                IsPassed = result.IsPassed,
                TimeTakenSeconds = result.TimeTakenSeconds,
                CompletedAt = result.CompletedAt,
                Message = message
            };
        }

        public async Task<ReviewQuizViewModel?> GetReviewAsync(Guid testResultId, Guid userId)
        {
            var result = await _testResultRepo.GetWithFullReviewAsync(testResultId, userId);
            if (result == null) return null;

            var model = new ReviewQuizViewModel
            {
                TestResultId = result.Id,
                LessonTitle = result.Lesson?.Title ?? "",
                Subject = result.Lesson?.Subject ?? "",
                TotalQuestions = result.TotalQuestions,
                CorrectAnswers = result.CorrectAnswers,
                Score = result.Score,
                IsPassed = result.IsPassed,
                Questions = new List<ReviewQuestionViewModel>()
            };

            int questionNum = 1;
            foreach (var detail in result.Details.OrderBy(d => d.Question?.OrderIndex))
            {
                if (detail.Question == null) continue;
                var correctAnswer = detail.Question.Answers.FirstOrDefault(a => a.IsCorrect);

                model.Questions.Add(new ReviewQuestionViewModel
                {
                    QuestionNumber = questionNum++,
                    QuestionText = detail.Question.QuestionText,
                    SelectedAnswerId = detail.SelectedAnswerId,
                    CorrectAnswerId = correctAnswer?.Id ?? Guid.Empty,
                    IsCorrect = detail.IsCorrect,
                    Answers = detail.Question.Answers
                        .OrderBy(a => a.AnswerLabel)
                        .Select(a => new ReviewAnswerViewModel
                        {
                            AnswerId = a.Id,
                            AnswerLabel = a.AnswerLabel,
                            AnswerText = a.AnswerText,
                            IsCorrect = a.IsCorrect,
                            IsSelected = detail.SelectedAnswerId == a.Id
                        }).ToList()
                });
            }

            return model;
        }

        public async Task<List<TestResult>> GetHistoryAsync(Guid userId)
        {
            return await _testResultRepo.GetRecentByStudentAsync(userId, int.MaxValue);
        }
    }
}
