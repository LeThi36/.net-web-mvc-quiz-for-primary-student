using Microsoft.AspNetCore.Mvc;
using HistoryGeoQuiz_PrimarySchool.Constants;
using HistoryGeoQuiz_PrimarySchool.Enums;
using HistoryGeoQuiz_PrimarySchool.Models;
using Microsoft.AspNetCore.Diagnostics;
using System.Diagnostics;

namespace HistoryGeoQuiz_PrimarySchool.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IWebHostEnvironment env, ILogger<HomeController> logger)
        {
            _env = env;
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var roleStr = GetCurrentUserRole();
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
        public IActionResult Error(int? statusCode = null)
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            
            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                IsDevelopment = _env.IsDevelopment(),
                StatusCode = statusCode?.ToString() ?? HttpContext.Response.StatusCode.ToString(),
                Path = exceptionHandlerPathFeature?.Path
            };

            if (exceptionHandlerPathFeature?.Error != null)
            {
                var ex = exceptionHandlerPathFeature.Error;
                if (_env.IsDevelopment())
                {
                    model.Message = ex.Message;
                    model.StackTrace = ex.StackTrace;
                }
                else
                {
                    model.Message = "Đã xảy ra lỗi không mong muốn trong quá trình xử lý yêu cầu của bạn.";
                }
                
                _logger.LogError(ex, "Error at {Path}", model.Path);
            }

            // Handle specific status codes if redirected from StatusCodePages
            if (statusCode.HasValue)
            {
                if (statusCode == 404)
                    model.Message = "Trang bạn đang tìm kiếm không tồn tại.";
                else if (statusCode == 403)
                    model.Message = "Bạn không có quyền truy cập vào khu vực này.";
            }

            return View(model);
        }
    }
}
