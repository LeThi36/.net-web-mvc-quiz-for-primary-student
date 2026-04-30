using Microsoft.AspNetCore.Mvc;
using HistoryGeoQuiz_PrimarySchool.Constants;
using HistoryGeoQuiz_PrimarySchool.Enums;
using HistoryGeoQuiz_PrimarySchool.Services.Interfaces;
using HistoryGeoQuiz_PrimarySchool.ViewModels;

namespace HistoryGeoQuiz_PrimarySchool.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthService authService, ILogger<AccountController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var userIdStr = HttpContext.Session.GetString(SessionKeys.UserId);
            if (!string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToDashboard(HttpContext.Session.GetString(SessionKeys.UserRole));
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _authService.LoginAsync(model.Username, model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng!");
                return View(model);
            }

            // Regenerate session to prevent session fixation
            HttpContext.Session.Clear();
            HttpContext.Session.SetString(SessionKeys.UserId, user.Id.ToString());
            HttpContext.Session.SetString(SessionKeys.UserName, user.FullName);
            HttpContext.Session.SetString(SessionKeys.UserRole, user.Role.ToString());

            return RedirectToDashboard(user.Role.ToString());
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, error) = await _authService.RegisterAsync(
                model.Username, model.Password, model.FullName, UserRole.Student);

            if (!success)
            {
                ModelState.AddModelError("", error!);
                return View(model);
            }

            TempData[TempDataKeys.SuccessMessage] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>Helper to redirect user to their role-specific dashboard.</summary>
        private IActionResult RedirectToDashboard(string? roleStr)
        {
            if (roleStr == nameof(UserRole.Admin)) return RedirectToAction("Index", "Class");
            if (roleStr == nameof(UserRole.Teacher)) return RedirectToAction("Index", "Teacher");
            
            return RedirectToAction("Index", "Student");
        }
    }
}
