using HistoryGeoQuiz_PrimarySchool.Repositories.Interfaces;
using HistoryGeoQuiz_PrimarySchool.Services.Interfaces.Analytic;
using HistoryGeoQuiz_PrimarySchool.ViewModels.Analytic;

namespace HistoryGeoQuiz_PrimarySchool.Services.Implement.Analytic
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ILessonRepository _lessonRepo;
        private readonly ITestResultRepository _testResultRepo;
        private readonly IUserRepository _userRepo;

        public AnalyticsService(
            ILessonRepository lessonRepo, 
            ITestResultRepository testResultRepo,
            IUserRepository userRepo)
        {
            _lessonRepo = lessonRepo;
            _testResultRepo = testResultRepo;
            _userRepo = userRepo;
        }

        public async Task<StudentReportViewModel?> GetStudentReportAsync(Guid lessonId, Guid studentId, Guid teacherId)
        {
            // 1. Verify lesson ownership
            var lesson = await _lessonRepo.GetByIdForTeacherAsync(lessonId, teacherId);
            if (lesson == null) return null;

            // 2. Get student info with class info
            var student = await _userRepo.GetStudentWithClassInfoAsync(studentId);
            if (student == null) return null;

            // 3. Get all attempts for this student in this lesson
            var allResultsForLesson = await _testResultRepo.GetResultsByLessonAsync(lessonId);
            var studentResults = allResultsForLesson
                .Where(r => r.StudentId == studentId)
                .OrderBy(r => r.CompletedAt)
                .ToList();

            if (!studentResults.Any()) return null;

            // 4. Calculate Class Average
            var classAverage = allResultsForLesson.Any() ? allResultsForLesson.Average(r => r.Score) : 0;

            // 5. Build ViewModel
            var bestResult = studentResults.OrderByDescending(r => r.Score).First();
            
            var viewModel = new StudentReportViewModel
            {
                StudentId = studentId,
                StudentName = student.FullName,
                ClassName = student.ClassRoom?.ClassName ?? "N/A",
                HomeroomTeacher = student.ClassRoom?.HomeroomTeacher?.FullName ?? "---",
                
                LessonId = lessonId,
                LessonTitle = lesson.Title,
                Subject = lesson.Subject,
                
                BestScore = bestResult.Score,
                ClassAverageScore = Math.Round(classAverage, 1),
                TotalAttempts = studentResults.Count,
                AverageTimeSeconds = (int)studentResults.Average(r => r.TimeTakenSeconds),
                
                CorrectAnswers = bestResult.CorrectAnswers,
                TotalQuestions = bestResult.TotalQuestions,
                
                AttemptHistory = studentResults.Select((r, index) => new AttemptHistoryViewModel
                {
                    AttemptNumber = index + 1,
                    Score = r.Score,
                    CompletedAt = r.CompletedAt
                }).ToList()
            };

            // Evaluation logic
            if (viewModel.BestScore >= 9) viewModel.Evaluation = "Xuất sắc";
            else if (viewModel.BestScore >= 8) viewModel.Evaluation = "Giỏi";
            else if (viewModel.BestScore >= 6.5) viewModel.Evaluation = "Khá";
            else if (viewModel.BestScore >= 5) viewModel.Evaluation = "Hoàn thành";
            else viewModel.Evaluation = "Cần cố gắng";

            return viewModel;
        }

        public async Task<QuestionAnalyticsViewModel?> GetLessonAnalyticsAsync(Guid lessonId, Guid teacherId)
        {
            // Verify lesson ownership
            var lesson = await _lessonRepo.GetByIdForTeacherAsync(lessonId, teacherId);
            
            if (lesson == null) return null;

            // Get all test results for summary metrics
            var results = await _testResultRepo.GetResultsByLessonAsync(lessonId);
            
            // Get all test details for question-specific stats
            var testDetails = await _testResultRepo.GetDetailsByLessonAsync(lessonId);

            var viewModel = new QuestionAnalyticsViewModel
            {
                LessonId = lessonId,
                LessonTitle = lesson.Title,
                TotalParticipants = results.Select(r => r.StudentId).Distinct().Count(),
                AverageScore = results.Any() ? Math.Round(results.Average(r => r.Score), 1) : 0,
                AverageTimeSeconds = results.Any() ? (int)results.Average(r => r.TimeTakenSeconds) : 0,
                PassCount = results.Count(r => r.IsPassed),
                FailCount = results.Count(r => !r.IsPassed),
                TopStudents = results.Take(5).Select(r => new StudentScoreViewModel
                {
                    StudentId = r.StudentId,
                    FullName = r.Student?.FullName ?? "N/A",
                    Score = r.Score,
                    TimeSeconds = r.TimeTakenSeconds
                }).ToList(),
                AllStudents = results.Select(r => new StudentScoreViewModel
                {
                    StudentId = r.StudentId,
                    FullName = r.Student?.FullName ?? "N/A",
                    Score = r.Score,
                    TimeSeconds = r.TimeTakenSeconds
                }).OrderByDescending(s => s.Score).ToList()
            };

            // Group by question
            var questionGroups = testDetails.GroupBy(d => d.QuestionId);

            foreach (var group in questionGroups)
            {
                var question = group.First().Question;
                if (question == null) continue;

                var totalAttempts = group.Count();
                var correctCount = group.Count(d => d.IsCorrect);
                var accuracyRate = totalAttempts > 0 ? (double)correctCount / totalAttempts * 100 : 0;

                viewModel.QuestionStats.Add(new QuestionStatViewModel
                {
                    QuestionId = group.Key,
                    QuestionText = question.QuestionText,
                    TotalAttempts = totalAttempts,
                    CorrectCount = correctCount,
                    IncorrectCount = totalAttempts - correctCount,
                    AccuracyRate = Math.Round(accuracyRate, 1)
                });
            }

            // Order by accuracy rate (hardest first)
            viewModel.QuestionStats = viewModel.QuestionStats.OrderBy(s => s.AccuracyRate).ToList();

            return viewModel;
        }
    }
}
