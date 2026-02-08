using Microsoft.AspNetCore.Mvc;
using HistoryGeoQuiz_PrimarySchool.Data;
using Microsoft.EntityFrameworkCore;

namespace HistoryGeoQuiz_PrimarySchool.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // If logged in, redirect to appropriate dashboard
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                var role = HttpContext.Session.GetString("UserRole");
                if (role == "Teacher")
                    return RedirectToAction("Index", "Teacher");
                else
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
