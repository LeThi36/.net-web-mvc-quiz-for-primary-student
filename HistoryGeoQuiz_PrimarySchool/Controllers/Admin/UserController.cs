using Microsoft.AspNetCore.Mvc;
using HistoryGeoQuiz_PrimarySchool.Enums;
using HistoryGeoQuiz_PrimarySchool.Filters;
using HistoryGeoQuiz_PrimarySchool.Services.Interfaces;
using HistoryGeoQuiz_PrimarySchool.ViewModels;
using ClosedXML.Excel;

namespace HistoryGeoQuiz_PrimarySchool.Controllers
{
    [AuthorizeRole(UserRole.Admin)]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        private const long MaxUploadFileSize = 10 * 1024 * 1024; // 10MB

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // GET: User/Index (List Teachers)
        public async Task<IActionResult> Index()
        {
            var teachers = await _userService.GetTeachersAsync();
            return View(teachers);
        }

        // GET: User/CreateTeacher
        public IActionResult CreateTeacher()
        {
            return View();
        }

        // POST: User/CreateTeacher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeacher(CreateTeacherViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, error) = await _userService.CreateTeacherAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Có lỗi xảy ra khi tạo giáo viên");
                return View(model);
            }

            TempData[HistoryGeoQuiz_PrimarySchool.Constants.TempDataKeys.SuccessMessage] = "Thêm giáo viên thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: User/ImportTeachers
        public IActionResult ImportTeachers()
        {
            return View(new ImportTeachersViewModel());
        }

        // POST: User/ImportTeachers
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportTeachers(ImportTeachersViewModel model)
        {
            if (model.ExcelFile == null || model.ExcelFile.Length == 0)
            {
                ModelState.AddModelError("ExcelFile", "Vui lòng chọn file Excel");
                return View(model);
            }

            if (model.ExcelFile.Length > MaxUploadFileSize)
            {
                ModelState.AddModelError("ExcelFile", "File không được vượt quá 5MB");
                return View(model);
            }

            var extension = Path.GetExtension(model.ExcelFile.FileName).ToLower();
            if (extension != ".xlsx")
            {
                ModelState.AddModelError("ExcelFile", "Chỉ hỗ trợ file Excel .xlsx");
                return View(model);
            }

            try
            {
                var result = await _userService.ImportTeachersFromExcelAsync(model.ExcelFile);
                return View("ImportTeachersResult", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing teachers");
                ModelState.AddModelError("ExcelFile", "Có lỗi xảy ra khi đọc file Excel. Vui lòng kiểm tra lại định dạng file.");
                return View(model);
            }
        }

        // GET: User/DownloadTeacherTemplate
        public IActionResult DownloadTeacherTemplate()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("DanhSachGiaoVien");

            // Header row
            worksheet.Cell(1, 1).Value = "Họ và tên";
            worksheet.Cell(1, 2).Value = "Tên đăng nhập";
            worksheet.Cell(1, 3).Value = "Mật khẩu";

            // Style header
            var headerRange = worksheet.Range(1, 1, 1, 3);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Example rows
            worksheet.Cell(2, 1).Value = "Giáo Viên A";
            worksheet.Cell(2, 2).Value = "giaoviena";
            worksheet.Cell(2, 3).Value = "123456";

            worksheet.Cell(3, 1).Value = "Giáo Viên B";
            worksheet.Cell(3, 2).Value = "giaovienb";
            worksheet.Cell(3, 3).Value = "123456";

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "MauDanhSachGiaoVien.xlsx"
            );
        }
    }
}
