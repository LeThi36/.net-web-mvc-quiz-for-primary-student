using Microsoft.AspNetCore.Mvc;
using HistoryGeoQuiz_PrimarySchool.Data;
using HistoryGeoQuiz_PrimarySchool.Models;
using HistoryGeoQuiz_PrimarySchool.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HistoryGeoQuiz_PrimarySchool.Controllers
{
    public class StudentController : Controller
    {
        private readonly AppDbContext _context;

        public StudentController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsStudent()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            return userId != null;
        }

        private int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        // GET: Student Dashboard
        public async Task<IActionResult> Index()
        {
            if (!IsStudent())
                return RedirectToAction("Login", "Account");

            var userId = GetCurrentUserId();

            // Get student's class
            var student = await _context.Users
                .Include(u => u.ClassRoom)
                    .ThenInclude(c => c.HomeroomTeacher)
                .FirstOrDefaultAsync(u => u.Id == userId);
            
            var classRoomId = student?.ClassRoomId;

            ViewBag.ClassName = student?.ClassRoom?.ClassName;
            ViewBag.HomeroomTeacher = student?.ClassRoom?.HomeroomTeacher?.FullName;

            // Get all active lessons for this class
            var lessons = await _context.Lessons
                .Where(l => l.IsActive && l.ClassRoomId == classRoomId)
                .Include(l => l.Questions)
                .Include(l => l.CreatedByUser) // Teacher info
                .OrderBy(l => l.Subject)
                .ThenBy(l => l.LessonNumber)
                .ToListAsync();

            // Get student's test results
            var testResults = await _context.TestResults
                .Where(tr => tr.StudentId == userId)
                .ToListAsync();

            var lessonCards = new List<LessonCardViewModel>();

            foreach (var lesson in lessons)
            {
                if (lesson.Questions.Count == 0) continue; // Skip lessons with no questions

                var studentResults = testResults.Where(tr => tr.LessonId == lesson.Id).ToList();
                var card = new LessonCardViewModel
                {
                    Id = lesson.Id,
                    Title = lesson.Title,
                    Description = lesson.Description,
                    Subject = lesson.Subject,
                    LessonNumber = lesson.LessonNumber,
                    QuestionCount = lesson.Questions.Count,
                    HasAttempted = studentResults.Any(),
                    BestScore = studentResults.Any() ? studentResults.Max(r => r.Score) : null
                };
                
                lessonCards.Add(card);
            }

            // Get recent results
            var recentResults = await _context.TestResults
                .Where(tr => tr.StudentId == userId)
                .Include(tr => tr.Lesson)
                .OrderByDescending(tr => tr.CompletedAt)
                .Take(5)
                .ToListAsync();

            var model = new StudentDashboardViewModel
            {
                Lessons = lessonCards,
                RecentResults = recentResults
            };

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(model);
        }

        // GET: Take Quiz
        public async Task<IActionResult> TakeQuiz(int lessonId)
        {
            if (!IsStudent())
                return RedirectToAction("Login", "Account");

            var userId = GetCurrentUserId();
            var student = await _context.Users.FindAsync(userId);

            var lesson = await _context.Lessons
                .Include(l => l.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.IsActive);
                
            // Security check: Student must be in the same class as the lesson
            if (lesson != null && lesson.ClassRoomId != student?.ClassRoomId)
            {
                return RedirectToAction("Index"); // Or Forbidden
            }

            if (lesson == null || !lesson.Questions.Any())
                return NotFound();

            var model = new TakeQuizViewModel
            {
                LessonId = lesson.Id,
                LessonTitle = lesson.Title,
                Subject = lesson.Subject,
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

            return View(model);
        }

        // POST: Submit Quiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQuiz(int lessonId, Dictionary<string, int> answers)
        {
            if (!IsStudent())
                return RedirectToAction("Login", "Account");

            var userId = GetCurrentUserId();

            var lesson = await _context.Lessons
                .Include(l => l.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null)
                return NotFound();

            // Convert string keys to int
            var answerDict = answers.ToDictionary(
                kvp => int.Parse(kvp.Key.Replace("answers[", "").Replace("]", "")),
                kvp => kvp.Value
            );

            int correctCount = 0;
            var testDetails = new List<TestDetail>();

            foreach (var question in lesson.Questions)
            {
                var selectedAnswerId = answerDict.ContainsKey(question.Id) ? answerDict[question.Id] : (int?)null;
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
            bool isPassed = score >= 5;

            var testResult = new TestResult
            {
                StudentId = userId,
                LessonId = lessonId,
                TotalQuestions = totalQuestions,
                CorrectAnswers = correctCount,
                Score = score,
                IsPassed = isPassed,
                CompletedAt = DateTime.UtcNow,
                TimeTakenSeconds = 0
            };

            _context.TestResults.Add(testResult);
            await _context.SaveChangesAsync();

            // Add test details
            foreach (var detail in testDetails)
            {
                detail.TestResultId = testResult.Id;
            }
            _context.TestDetails.AddRange(testDetails);
            await _context.SaveChangesAsync();

            return RedirectToAction("Result", new { testResultId = testResult.Id });
        }

        // GET: View Result
        public async Task<IActionResult> Result(int testResultId)
        {
            if (!IsStudent())
                return RedirectToAction("Login", "Account");

            var userId = GetCurrentUserId();

            var result = await _context.TestResults
                .Include(tr => tr.Lesson)
                .FirstOrDefaultAsync(tr => tr.Id == testResultId && tr.StudentId == userId);

            if (result == null)
                return NotFound();

            // Grading Logic
            int excellentThreshold = result.Lesson?.QuestionCountForExcellent ?? (int)Math.Ceiling(result.TotalQuestions * 0.9);
            int goodThreshold = result.Lesson?.QuestionCountForGood ?? (int)Math.Ceiling(result.TotalQuestions * 0.7);

            string message;
            if (result.CorrectAnswers >= excellentThreshold)
                message = "🌟 Hoàn thành xuất sắc! Con giỏi lắm! 🌟"; // "Excellent"
            else if (result.CorrectAnswers >= goodThreshold)
                message = "👏 Hoàn thành tốt! Con làm tốt lắm! 👏"; // "Good"
            else if (result.Score >= 5)
                message = "😊 Hoàn thành! Cố gắng thêm nhé! 😊"; // "Pass" based on score or other criteria? Score >=5 is standard.
            else
                message = "💪 Chưa đạt! Con ôn bài và làm lại nhé! 💪";

            var model = new QuizResultViewModel
            {
                TestResultId = result.Id,
                LessonTitle = result.Lesson?.Title ?? "",
                Subject = result.Lesson?.Subject ?? "",
                TotalQuestions = result.TotalQuestions,
                CorrectAnswers = result.CorrectAnswers,
                Score = result.Score,
                IsPassed = result.IsPassed,
                CompletedAt = result.CompletedAt,
                Message = message
            };

            return View(model);
        }

        // GET: Review Quiz
        public async Task<IActionResult> ReviewQuiz(int testResultId)
        {
            if (!IsStudent())
                return RedirectToAction("Login", "Account");

            var userId = GetCurrentUserId();

            var result = await _context.TestResults
                .Include(tr => tr.Lesson)
                .Include(tr => tr.Details)
                    .ThenInclude(d => d.Question)
                        .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(tr => tr.Id == testResultId && tr.StudentId == userId);

            if (result == null)
                return NotFound();

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
                    CorrectAnswerId = correctAnswer?.Id ?? 0,
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

            return View(model);
        }

        // GET: History of all tests
        public async Task<IActionResult> History()
        {
            if (!IsStudent())
                return RedirectToAction("Login", "Account");

            var userId = GetCurrentUserId();

            var results = await _context.TestResults
                .Where(tr => tr.StudentId == userId)
                .Include(tr => tr.Lesson)
                .OrderByDescending(tr => tr.CompletedAt)
                .ToListAsync();

            return View(results);
        }
    }
}
