using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HistoryGeoQuiz_PrimarySchool.Constants;
using HistoryGeoQuiz_PrimarySchool.Enums;
using HistoryGeoQuiz_PrimarySchool.Filters;
using HistoryGeoQuiz_PrimarySchool.Helpers;
using HistoryGeoQuiz_PrimarySchool.Services.Interfaces;
using HistoryGeoQuiz_PrimarySchool.ViewModels;
using HistoryGeoQuiz_PrimarySchool.ViewModels.Teacher;
using HistoryGeoQuiz_PrimarySchool.ViewModels.Analytic;
using Microsoft.AspNetCore.Mvc.Rendering;
using HistoryGeoQuiz_PrimarySchool.Services.Interfaces.Analytic;

namespace HistoryGeoQuiz_PrimarySchool.Controllers
{
    [AuthorizeRole(UserRole.Teacher)]
    public class TeacherController : BaseController
    {
        private readonly ILessonService _lessonService;
        private readonly IQuestionService _questionService;
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<TeacherController> _logger;

        private const long MaxUploadFileSize = 5 * 1024 * 1024; // 5MB

        public TeacherController(
            ILessonService lessonService,
            IQuestionService questionService,
            IAnalyticsService analyticsService,
            ILogger<TeacherController> logger)
        {
            _lessonService = lessonService;
            _questionService = questionService;
            _analyticsService = analyticsService;
            _logger = logger;
        }

        // GET: Teacher Dashboard
        public async Task<IActionResult> Index()
        {
            var lessons = await _lessonService.GetLessonsByTeacherAsync(GetCurrentUserId());
            ViewBag.UserName = GetCurrentUserName();
            return View(lessons);
        }

        // Note: MyClasses, GetSubjectsByClass still use DbContext directly
        // because they don't have a dedicated service yet (ClassService handles class management for Admin).
        // These will be migrated when the full ClassService is implemented.

        #region Lesson CRUD

        [HttpGet]
        public async Task<IActionResult> CreateLesson()
        {
            await PopulateClassRoomsDropdown();
            return View(new CreateLessonViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLesson(CreateLessonViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateClassRoomsDropdown(model.ClassRoomId);
                return View(model);
            }

            try
            {
                await _lessonService.CreateLessonAsync(model, GetCurrentUserId());
                TempData[TempDataKeys.SuccessMessage] = "Tạo bài học thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lesson");
                TempData[TempDataKeys.ErrorMessage] = "Có lỗi xảy ra khi lưu bài học. Vui lòng thử lại sau.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> EditLesson(Guid id)
        {
            var lesson = await _lessonService.GetLessonByIdAsync(id, GetCurrentUserId());
            if (lesson == null) return NotFound();

            var model = new CreateLessonViewModel
            {
                Title = lesson.Title,
                Description = lesson.Description,
                Subject = lesson.Subject,
                LessonNumber = lesson.LessonNumber,
                ClassRoomId = lesson.ClassRoomId ?? Guid.Empty,
                QuestionCountForExcellent = lesson.QuestionCountForExcellent,
                QuestionCountForGood = lesson.QuestionCountForGood
            };

            await PopulateClassRoomsDropdown(model.ClassRoomId);
            ViewBag.LessonId = id;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLesson(Guid id, CreateLessonViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateClassRoomsDropdown(model.ClassRoomId);
                ViewBag.LessonId = id;
                return View(model);
            }

            if (!await _lessonService.UpdateLessonAsync(id, model, GetCurrentUserId()))
                return NotFound();

            TempData[TempDataKeys.SuccessMessage] = "Cập nhật bài học thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLesson(Guid id)
        {
            if (!await _lessonService.DeleteLessonAsync(id, GetCurrentUserId()))
                return NotFound();

            TempData[TempDataKeys.SuccessMessage] = "Xóa bài học thành công!";
            return RedirectToAction("Index");
        }

        #endregion

        #region Question CRUD

        public async Task<IActionResult> ManageQuestions(Guid lessonId)
        {
            var lesson = await _lessonService.GetLessonDetailWithQuestionsAsync(lessonId, GetCurrentUserId());
            if (lesson == null) return NotFound();

            var model = new LessonDetailViewModel
            {
                Lesson = lesson,
                Questions = lesson.Questions.OrderBy(q => q.OrderIndex).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLessonConfig(Guid lessonId, int? timeLimit, int? excellent, int? good)
        {
            if (!await _lessonService.UpdateLessonConfigAsync(lessonId, timeLimit, excellent, good, GetCurrentUserId()))
                return NotFound();

            TempData[TempDataKeys.SuccessMessage] = "Cập nhật cấu hình bài học thành công!";
            return RedirectToAction("ManageQuestions", new { lessonId });
        }

        [HttpGet]
        public async Task<IActionResult> CreateQuestion(Guid lessonId)
        {
            var lesson = await _lessonService.GetLessonByIdAsync(lessonId, GetCurrentUserId());
            if (lesson == null) return NotFound();

            return View(new CreateQuestionViewModel { LessonId = lessonId, LessonTitle = lesson.Title });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuestion(CreateQuestionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var lesson = await _lessonService.GetLessonByIdAsync(model.LessonId, GetCurrentUserId());
                model.LessonTitle = lesson?.Title;
                return View(model);
            }

            try
            {
                await _questionService.CreateQuestionAsync(model, GetCurrentUserId());
                TempData[TempDataKeys.SuccessMessage] = "Thêm câu hỏi thành công!";
            }
            catch (UnauthorizedAccessException)
            {
                return NotFound();
            }

            return RedirectToAction("ManageQuestions", new { lessonId = model.LessonId });
        }

        [HttpGet]
        public async Task<IActionResult> EditQuestion(Guid id)
        {
            var question = await _questionService.GetQuestionForEditAsync(id, GetCurrentUserId());
            if (question == null) return NotFound();

            var answers = question.Answers.ToList();
            var model = new EditQuestionViewModel
            {
                Id = question.Id,
                LessonId = question.LessonId,
                LessonTitle = question.Lesson?.Title,
                QuestionText = question.QuestionText,
                AnswerA = answers.FirstOrDefault(a => a.AnswerLabel == "A")?.AnswerText ?? "",
                AnswerB = answers.FirstOrDefault(a => a.AnswerLabel == "B")?.AnswerText ?? "",
                AnswerC = answers.FirstOrDefault(a => a.AnswerLabel == "C")?.AnswerText ?? "",
                AnswerD = answers.FirstOrDefault(a => a.AnswerLabel == "D")?.AnswerText ?? "",
                CorrectAnswer = answers.FirstOrDefault(a => a.IsCorrect)?.AnswerLabel ?? "A"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestion(EditQuestionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var q = await _questionService.GetQuestionForEditAsync(model.Id, GetCurrentUserId());
                model.LessonTitle = q?.Lesson?.Title;
                return View(model);
            }

            if (!await _questionService.UpdateQuestionAsync(model, GetCurrentUserId()))
                return NotFound();

            TempData[TempDataKeys.SuccessMessage] = "Cập nhật câu hỏi thành công!";
            return RedirectToAction("ManageQuestions", new { lessonId = model.LessonId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(Guid id)
        {
            var question = await _questionService.GetQuestionForEditAsync(id, GetCurrentUserId());
            if (question == null) return NotFound();

            var lessonId = question.LessonId;
            await _questionService.DeleteQuestionAsync(id, GetCurrentUserId());

            TempData[TempDataKeys.SuccessMessage] = "Xóa câu hỏi thành công!";
            return RedirectToAction("ManageQuestions", new { lessonId });
        }

        public async Task<IActionResult> QuestionAnalytics(Guid lessonId)
        {
            var analytics = await _analyticsService.GetLessonAnalyticsAsync(lessonId, GetCurrentUserId());
            if (analytics == null) return NotFound();

            return View(analytics);
        }

        public async Task<IActionResult> StudentReport(Guid lessonId, Guid studentId)
        {
            var report = await _analyticsService.GetStudentReportAsync(lessonId, studentId, GetCurrentUserId());
            if (report == null) return NotFound();

            return View(report);
        }

        #endregion

        #region Excel Upload/Download

        [HttpGet]
        public IActionResult UploadQuestions(Guid lessonId)
        {
            ViewBag.LessonId = lessonId;
            return View();
        }

        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("CauHoi");

            worksheet.Cell(1, 1).Value = "NoiDungCauHoi";
            worksheet.Cell(1, 2).Value = "DapAnA";
            worksheet.Cell(1, 3).Value = "DapAnB";
            worksheet.Cell(1, 4).Value = "DapAnC";
            worksheet.Cell(1, 5).Value = "DapAnD";
            worksheet.Cell(1, 6).Value = "DapAnDung";

            worksheet.Cell(2, 1).Value = "Vua Hùng đầu tiên có tên hiệu là gì?";
            worksheet.Cell(2, 2).Value = "Kinh Dương Vương";
            worksheet.Cell(2, 3).Value = "Lạc Long Quân";
            worksheet.Cell(2, 4).Value = "Âu Cơ";
            worksheet.Cell(2, 5).Value = "Hùng Vương";
            worksheet.Cell(2, 6).Value = "A";

            var headerRange = worksheet.Range(1, 1, 1, 6);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGreen;
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "mau_cau_hoi.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadQuestions(Guid lessonId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn file Excel (.xlsx)");
                ViewBag.LessonId = lessonId;
                return View();
            }

            if (file.Length > MaxUploadFileSize)
            {
                ModelState.AddModelError("", "File không được vượt quá 5MB");
                ViewBag.LessonId = lessonId;
                return View();
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
            {
                ModelState.AddModelError("", "Chỉ hỗ trợ file Excel (.xlsx hoặc .xls)");
                ViewBag.LessonId = lessonId;
                return View();
            }

            try
            {
                var count = await _questionService.ImportQuestionsFromExcelAsync(
                    lessonId, file, GetCurrentUserId());
                TempData[TempDataKeys.SuccessMessage] = $"Đã import {count} câu hỏi thành công!";
            }
            catch (UnauthorizedAccessException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing questions for lesson {LessonId}", lessonId);
                ModelState.AddModelError("", "Có lỗi xảy ra khi import file. Vui lòng kiểm tra lại định dạng file.");
                ViewBag.LessonId = lessonId;
                return View();
            }

            return RedirectToAction("ManageQuestions", new { lessonId });
        }

        #endregion

        #region API Endpoints

        [HttpGet]
        public async Task<IActionResult> GetSubjectsByClass(Guid classRoomId)
        {
            // This still queries DB directly - will be moved to a TeacherService in future
            var userId = GetCurrentUserId();
            using var scope = HttpContext.RequestServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();

            var subjects = await context.TeacherAssignments
                .Where(ta => ta.TeacherId == userId && ta.ClassRoomId == classRoomId)
                .Select(ta => ta.Subject)
                .Distinct()
                .ToListAsync();

            var subjectList = subjects.Select(s => new
            {
                Value = s,
                Text = $"{SubjectHelper.GetIcon(s)} {SubjectHelper.GetName(s)}"
            }).ToList();

            return Json(subjectList);
        }

        // GET: My Classes
        public async Task<IActionResult> MyClasses()
        {
            using var scope = HttpContext.RequestServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();

            var assignments = await context.TeacherAssignments
                .Include(ta => ta.ClassRoom)
                    .ThenInclude(c => c.Students)
                .Where(ta => ta.TeacherId == GetCurrentUserId())
                .ToListAsync();

            return View(assignments);
        }

        #endregion

        #region Helpers

        private async Task PopulateClassRoomsDropdown(Guid? selectedClassRoomId = null)
        {
            using var scope = HttpContext.RequestServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();

            var assignedClasses = await context.TeacherAssignments
                .Include(ta => ta.ClassRoom)
                .Where(ta => ta.TeacherId == GetCurrentUserId())
                .Select(ta => new { ta.ClassRoomId, ta.ClassRoom.ClassName })
                .Distinct()
                .ToListAsync();

            ViewBag.ClassRooms = new SelectList(assignedClasses, "ClassRoomId", "ClassName", selectedClassRoomId);
        }

        #endregion
    }
}
