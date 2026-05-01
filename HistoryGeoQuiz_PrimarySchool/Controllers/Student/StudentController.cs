using Microsoft.AspNetCore.Mvc;
using HistoryGeoQuiz_PrimarySchool.Constants;
using HistoryGeoQuiz_PrimarySchool.Enums;
using HistoryGeoQuiz_PrimarySchool.Filters;
using HistoryGeoQuiz_PrimarySchool.Services.Interfaces;

namespace HistoryGeoQuiz_PrimarySchool.Controllers
{
    [AuthorizeRole(UserRole.Student)]
    public class StudentController : BaseController
    {
        private readonly ITestService _testService;

        public StudentController(ITestService testService)
        {
            _testService = testService;
        }

        // GET: Student Dashboard
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var (_, className, homeroomTeacher) = await _testService.GetStudentInfoAsync(userId);

            ViewBag.ClassName = className;
            ViewBag.HomeroomTeacher = homeroomTeacher;
            ViewBag.UserName = HttpContext.Session.GetString(SessionKeys.UserName);

            var model = await _testService.GetStudentDashboardAsync(userId);
            return View(model);
        }

        // GET: Take Quiz
        public async Task<IActionResult> TakeQuiz(Guid lessonId)
        {
            var model = await _testService.GetQuizAsync(lessonId, GetCurrentUserId());
            if (model == null) return NotFound();

            return View(model);
        }

        // POST: Submit Quiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQuiz(Guid lessonId, Dictionary<string, Guid> answers, DateTime startTime)
        {
            var timeTakenSeconds = (int)(DateTime.UtcNow - startTime).TotalSeconds;
            if (timeTakenSeconds < 0) timeTakenSeconds = 0;

            var testResult = await _testService.SubmitQuizAsync(lessonId, answers, GetCurrentUserId(), timeTakenSeconds);
            return RedirectToAction("Result", new { testResultId = testResult.Id });
        }

        // GET: View Result
        public async Task<IActionResult> Result(Guid testResultId)
        {
            var model = await _testService.GetResultAsync(testResultId, GetCurrentUserId());
            if (model == null) return NotFound();

            return View(model);
        }

        // GET: Review Quiz
        public async Task<IActionResult> ReviewQuiz(Guid testResultId)
        {
            var model = await _testService.GetReviewAsync(testResultId, GetCurrentUserId());
            if (model == null) return NotFound();

            return View(model);
        }

        // GET: History
        public async Task<IActionResult> History()
        {
            var results = await _testService.GetHistoryAsync(GetCurrentUserId());
            return View(results);
        }
    }
}
