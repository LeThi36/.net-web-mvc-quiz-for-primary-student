using Microsoft.AspNetCore.Mvc;
using HistoryGeoQuiz_PrimarySchool.Constants;
using HistoryGeoQuiz_PrimarySchool.Enums;

namespace HistoryGeoQuiz_PrimarySchool.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            var userIdStr = HttpContext.Session.GetString(SessionKeys.UserId);
            if (!string.IsNullOrEmpty(userIdStr))
            {
                var roleStr = HttpContext.Session.GetString(SessionKeys.UserRole);
                if (roleStr == nameof(UserRole.Admin)) return RedirectToAction("Index", "Class");
                if (roleStr == nameof(UserRole.Teacher)) return RedirectToAction("Index", "Teacher");
                return RedirectToAction("Index", "Student");
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
