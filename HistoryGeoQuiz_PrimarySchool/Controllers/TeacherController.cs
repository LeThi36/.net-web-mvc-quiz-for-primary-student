using Microsoft.AspNetCore.Mvc;
using HistoryGeoQuiz_PrimarySchool.Data;
using HistoryGeoQuiz_PrimarySchool.Models;
using HistoryGeoQuiz_PrimarySchool.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.IO;
using ExcelDataReader;

namespace HistoryGeoQuiz_PrimarySchool.Controllers
{
    public class TeacherController : Controller
    {
        private readonly AppDbContext _context;

        public TeacherController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsTeacher()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role == "Teacher";
        }

        private int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        // GET: Teacher Dashboard
        public async Task<IActionResult> Index()
        {
            if (!IsTeacher())
                return RedirectToAction("Login", "Account");

            var userId = GetCurrentUserId();

            // Only show lessons for classes assigned to this teacher
            // Or show all lessons created by this teacher
            // Requirement: "giáo viên dạy môn nào? lớp nào? khi đăng nhập vào sẽ chỉ quản lý được lớp của mình thôi"
            
            // Get list of ClassIds assigned to this teacher
            var assignedClassIds = await _context.TeacherAssignments
                .Where(ta => ta.TeacherId == userId)
                .Select(ta => ta.ClassRoomId)
                .ToListAsync();

            var lessons = await _context.Lessons
                .Where(l => l.CreatedByUserId == userId)
                .Include(l => l.Questions)
                .Include(l => l.ClassRoom) // Include ClassRoom info
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(lessons);
        }

        // GET: My Classes
        public async Task<IActionResult> MyClasses()
        {
            if (!IsTeacher()) return RedirectToAction("Login", "Account");
            var userId = GetCurrentUserId();

            var assignments = await _context.TeacherAssignments
                .Include(ta => ta.ClassRoom)
                    .ThenInclude(c => c.Students)
                .Where(ta => ta.TeacherId == userId)
                .ToListAsync();

            return View(assignments);
        }

        // GET: Create Lesson
        [HttpGet]
        public async Task<IActionResult> CreateLesson()
        {
            if (!IsTeacher())
                return RedirectToAction("Login", "Account");

            var userId = GetCurrentUserId();
            // Get classes assigned to teacher (distinct to avoid duplicates when teacher has multiple subjects in same class)
            var assignedClasses = await _context.TeacherAssignments
                .Include(ta => ta.ClassRoom)
                .Where(ta => ta.TeacherId == userId)
                .Select(ta => new { ta.ClassRoomId, ta.ClassRoom.ClassName })
                .Distinct()
                .ToListAsync();

            ViewBag.ClassRooms = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(assignedClasses, "ClassRoomId", "ClassName");
            
            return View(new CreateLessonViewModel());
        }

        // POST: Create Lesson
        [HttpPost]
        // [ValidateAntiForgeryToken] // TODO: Temporarily disabled for debugging
        public async Task<IActionResult> CreateLesson(CreateLessonViewModel model)
        {
            if (!IsTeacher())
                return RedirectToAction("Login", "Account");

            // DEBUG: Log the incoming request
            System.Diagnostics.Debug.WriteLine($"[DEBUG] CreateLesson POST - Subject: {model.Subject}, Title: {model.Title}, ClassRoomId: {model.ClassRoomId}");
            Console.WriteLine($"[DEBUG] CreateLesson POST - Subject: {model.Subject}, Title: {model.Title}, ClassRoomId: {model.ClassRoomId}");

            if (!ModelState.IsValid)
            {
                // DEBUG: Log validation errors
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                System.Diagnostics.Debug.WriteLine($"[DEBUG] ModelState Invalid - Errors: {errors}");
                Console.WriteLine($"[DEBUG] ModelState Invalid - Errors: {errors}");
                
                 var userId = GetCurrentUserId();
                var assignedClasses = await _context.TeacherAssignments
                    .Include(ta => ta.ClassRoom)
                    .Where(ta => ta.TeacherId == userId)
                    .Select(ta => new { ta.ClassRoomId, ta.ClassRoom.ClassName })
                    .ToListAsync();
                ViewBag.ClassRooms = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(assignedClasses, "ClassRoomId", "ClassName", model.ClassRoomId);
                return View(model);
            }

            var lesson = new Lesson
            {
                Title = model.Title,
                Description = model.Description,
                Subject = model.Subject,
                LessonNumber = model.LessonNumber,
                CreatedByUserId = GetCurrentUserId(),
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                ClassRoomId = model.ClassRoomId,
                QuestionCountForExcellent = model.QuestionCountForExcellent,
                QuestionCountForGood = model.QuestionCountForGood
            };

            _context.Lessons.Add(lesson);
            
            try
            {
                var rowsAffected = await _context.SaveChangesAsync();
                
                // DEBUG: Confirm save
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Lesson saved - ID: {lesson.Id}, Subject: {lesson.Subject}, RowsAffected: {rowsAffected}");
                Console.WriteLine($"[DEBUG] Lesson saved - ID: {lesson.Id}, Subject: {lesson.Subject}, RowsAffected: {rowsAffected}");
                
                if (rowsAffected == 0)
                {
                    System.Diagnostics.Debug.WriteLine("[ERROR] SaveChanges returned 0 rows affected!");
                    Console.WriteLine("[ERROR] SaveChanges returned 0 rows affected!");
                    TempData["ErrorMessage"] = "Lỗi: Không thể lưu bài học vào database!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                // DEBUG: Log exception
                System.Diagnostics.Debug.WriteLine($"[ERROR] SaveChanges exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine($"[ERROR] InnerException: {ex.InnerException?.Message}");
                Console.WriteLine($"[ERROR] SaveChanges exception: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                Console.WriteLine($"[ERROR] InnerException: {ex.InnerException?.Message}");
                
                TempData["ErrorMessage"] = $"Lỗi khi lưu bài học: {ex.Message}";
                return RedirectToAction("Index");
            }

            TempData["SuccessMessage"] = "Tạo bài học thành công!";
            return RedirectToAction("Index");
        }

        // GET: Edit Lesson
        [HttpGet]
        public async Task<IActionResult> EditLesson(int id)
        {
            if (!IsTeacher())
                return RedirectToAction("Login", "Account");

            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null || lesson.CreatedByUserId != GetCurrentUserId())
                return NotFound();

            var model = new CreateLessonViewModel
            {
                Title = lesson.Title,
                Description = lesson.Description,
                Subject = lesson.Subject,
                LessonNumber = lesson.LessonNumber
            };

            ViewBag.LessonId = id;
            return View(model);
        }

        // POST: Edit Lesson
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLesson(int id, CreateLessonViewModel model)
        {
            if (!IsTeacher())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                ViewBag.LessonId = id;
                return View(model);
            }

            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null || lesson.CreatedByUserId != GetCurrentUserId())
                return NotFound();

            lesson.Title = model.Title;
            lesson.Description = model.Description;
            lesson.Subject = model.Subject;
            lesson.LessonNumber = model.LessonNumber;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật bài học thành công!";
            return RedirectToAction("Index");
        }

        // POST: Delete Lesson
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            if (!IsTeacher())
                return RedirectToAction("Login", "Account");

            var lesson = await _context.Lessons
                .Include(l => l.Questions)
                    .ThenInclude(q => q.Answers)
                .Include(l => l.Questions)
                    .ThenInclude(q => q.TestDetails)
                .Include(l => l.TestResults)
                    .ThenInclude(tr => tr.Details)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null || lesson.CreatedByUserId != GetCurrentUserId())
                return NotFound();

            // Remove all test details first
            foreach (var question in lesson.Questions)
            {
                if (question.TestDetails != null && question.TestDetails.Any())
                {
                    _context.TestDetails.RemoveRange(question.TestDetails);
                }
            }

            // Remove all test results
            if (lesson.TestResults != null && lesson.TestResults.Any())
            {
                _context.TestResults.RemoveRange(lesson.TestResults);
            }

            // Now remove the lesson (will cascade delete Questions and Answers)
            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa bài học thành công!";
            return RedirectToAction("Index");
        }

        // GET: Manage Questions for a Lesson
        public async Task<IActionResult> ManageQuestions(int lessonId)
        {
            if (!IsTeacher())
                return RedirectToAction("Login", "Account");

            var lesson = await _context.Lessons
                .Include(l => l.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null || lesson.CreatedByUserId != GetCurrentUserId())
                return NotFound();

            var model = new LessonDetailViewModel
            {
                Lesson = lesson,
                Questions = lesson.Questions.OrderBy(q => q.OrderIndex).ToList()
            };

            return View(model);
        }

        // GET: Create Question
        [HttpGet]
        public async Task<IActionResult> CreateQuestion(int lessonId)
        {
            if (!IsTeacher())
                return RedirectToAction("Login", "Account");

            var lesson = await _context.Lessons.FindAsync(lessonId);
            if (lesson == null || lesson.CreatedByUserId != GetCurrentUserId())
                return NotFound();

            var model = new CreateQuestionViewModel
            {
                LessonId = lessonId,
                LessonTitle = lesson.Title
            };

            return View(model);
        }

        // POST: Create Question
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuestion(CreateQuestionViewModel model)
        {
            if (!IsTeacher())
                return RedirectToAction("Login", "Account");

            var lesson = await _context.Lessons.FindAsync(model.LessonId);
            if (lesson == null || lesson.CreatedByUserId != GetCurrentUserId())
                return NotFound();

            if (!ModelState.IsValid)
            {
                model.LessonTitle = lesson.Title;
                return View(model);
            }

            // Get next order index
            var maxOrder = await _context.Questions
                .Where(q => q.LessonId == model.LessonId)
                .MaxAsync(q => (int?)q.OrderIndex) ?? 0;

            var question = new Question
            {
                LessonId = model.LessonId,
                QuestionText = model.QuestionText,
                OrderIndex = maxOrder + 1,
                CreatedAt = DateTime.UtcNow
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            // Add 4 answers
            var answers = new List<Answer>
            {
                new Answer { QuestionId = question.Id, AnswerText = model.AnswerA, AnswerLabel = "A", IsCorrect = model.CorrectAnswer == "A" },
                new Answer { QuestionId = question.Id, AnswerText = model.AnswerB, AnswerLabel = "B", IsCorrect = model.CorrectAnswer == "B" },
                new Answer { QuestionId = question.Id, AnswerText = model.AnswerC, AnswerLabel = "C", IsCorrect = model.CorrectAnswer == "C" },
                new Answer { QuestionId = question.Id, AnswerText = model.AnswerD, AnswerLabel = "D", IsCorrect = model.CorrectAnswer == "D" }
            };

            _context.Answers.AddRange(answers);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thêm câu hỏi thành công!";
            return RedirectToAction("ManageQuestions", new { lessonId = model.LessonId });
        }

        // GET: Edit Question
        [HttpGet]
        public async Task<IActionResult> EditQuestion(int id)
        {
            if (!IsTeacher())
                return RedirectToAction("Login", "Account");

            var question = await _context.Questions
                .Include(q => q.Lesson)
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null || question.Lesson?.CreatedByUserId != GetCurrentUserId())
                return NotFound();

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

        // POST: Edit Question
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestion(EditQuestionViewModel model)
        {
            if (!IsTeacher())
                return RedirectToAction("Login", "Account");

            var question = await _context.Questions
                .Include(q => q.Lesson)
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == model.Id);

            if (question == null || question.Lesson?.CreatedByUserId != GetCurrentUserId())
                return NotFound();

            if (!ModelState.IsValid)
            {
                model.LessonTitle = question.Lesson?.Title;
                return View(model);
            }

            question.QuestionText = model.QuestionText;

            // Update answers
            var answers = question.Answers.ToList();
            foreach (var answer in answers)
            {
                switch (answer.AnswerLabel)
                {
                    case "A":
                        answer.AnswerText = model.AnswerA;
                        answer.IsCorrect = model.CorrectAnswer == "A";
                        break;
                    case "B":
                        answer.AnswerText = model.AnswerB;
                        answer.IsCorrect = model.CorrectAnswer == "B";
                        break;
                    case "C":
                        answer.AnswerText = model.AnswerC;
                        answer.IsCorrect = model.CorrectAnswer == "C";
                        break;
                    case "D":
                        answer.AnswerText = model.AnswerD;
                        answer.IsCorrect = model.CorrectAnswer == "D";
                        break;
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật câu hỏi thành công!";
            return RedirectToAction("ManageQuestions", new { lessonId = model.LessonId });
        }

        // POST: Delete Question
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            if (!IsTeacher())
                return RedirectToAction("Login", "Account");

            var question = await _context.Questions
                .Include(q => q.Lesson)
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null || question.Lesson?.CreatedByUserId != GetCurrentUserId())
                return NotFound();

            var lessonId = question.LessonId;
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa câu hỏi thành công!";
            return RedirectToAction("ManageQuestions", new { lessonId });
        }
        // GET: Upload Questions
        [HttpGet]
        public IActionResult UploadQuestions(int lessonId)
        {
             if (!IsTeacher()) return RedirectToAction("Login", "Account");
             ViewBag.LessonId = lessonId;
             return View();
        }

        // GET: Download Excel Template
        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            if (!IsTeacher()) return RedirectToAction("Login", "Account");
            
            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("CauHoi");
                
                // Header row
                worksheet.Cell(1, 1).Value = "NoiDungCauHoi";
                worksheet.Cell(1, 2).Value = "DapAnA";
                worksheet.Cell(1, 3).Value = "DapAnB";
                worksheet.Cell(1, 4).Value = "DapAnC";
                worksheet.Cell(1, 5).Value = "DapAnD";
                worksheet.Cell(1, 6).Value = "DapAnDung";
                
                // Sample row
                worksheet.Cell(2, 1).Value = "Vua Hùng đầu tiên có tên hiệu là gì?";
                worksheet.Cell(2, 2).Value = "Kinh Dương Vương";
                worksheet.Cell(2, 3).Value = "Lạc Long Quân";
                worksheet.Cell(2, 4).Value = "Âu Cơ";
                worksheet.Cell(2, 5).Value = "Hùng Vương";
                worksheet.Cell(2, 6).Value = "A";
                
                // Style header
                var headerRange = worksheet.Range(1, 1, 1, 6);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGreen;
                
                worksheet.Columns().AdjustToContents();
                
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "mau_cau_hoi.xlsx");
                }
            }
        }

        // POST: Upload Questions (Excel .xlsx only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadQuestions(int lessonId, IFormFile file)
        {
            if (!IsTeacher()) return RedirectToAction("Login", "Account");

            var lesson = await _context.Lessons.FindAsync(lessonId);
            if (lesson == null || lesson.CreatedByUserId != GetCurrentUserId()) return NotFound();

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn file Excel (.xlsx)");
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
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                
                int count = 0;
                var maxOrder = await _context.Questions.Where(q => q.LessonId == lessonId).MaxAsync(q => (int?)q.OrderIndex) ?? 0;

                using (var stream = file.OpenReadStream())
                using (var excelReader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream))
                {
                    var dataSet = excelReader.AsDataSet(new ExcelDataReader.ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataReader.ExcelDataTableConfiguration() { UseHeaderRow = true }
                    });
                    
                    if (dataSet.Tables.Count > 0)
                    {
                        var table = dataSet.Tables[0];
                        foreach (System.Data.DataRow row in table.Rows)
                        {
                            string GetCell(params string[] possibleNames)
                            {
                                foreach (var name in possibleNames)
                                {
                                    if (table.Columns.Contains(name))
                                        return row[name]?.ToString() ?? "";
                                }
                                return "";
                            }

                            string qText = GetCell("NoiDungCauHoi", "QuestionText");
                            if (string.IsNullOrWhiteSpace(qText)) continue;

                            var question = new Question
                            {
                                LessonId = lessonId,
                                QuestionText = qText,
                                OrderIndex = maxOrder + (++count),
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.Questions.Add(question);
                            await _context.SaveChangesAsync();

                            string ansA = GetCell("DapAnA", "AnswerA");
                            string ansB = GetCell("DapAnB", "AnswerB");
                            string ansC = GetCell("DapAnC", "AnswerC");
                            string ansD = GetCell("DapAnD", "AnswerD");
                            string correct = GetCell("DapAnDung", "CorrectAnswer").Trim().ToUpper();

                            _context.Answers.Add(new Answer { QuestionId = question.Id, AnswerText = ansA, AnswerLabel = "A", IsCorrect = correct == "A" });
                            _context.Answers.Add(new Answer { QuestionId = question.Id, AnswerText = ansB, AnswerLabel = "B", IsCorrect = correct == "B" });
                            _context.Answers.Add(new Answer { QuestionId = question.Id, AnswerText = ansC, AnswerLabel = "C", IsCorrect = correct == "C" });
                            _context.Answers.Add(new Answer { QuestionId = question.Id, AnswerText = ansD, AnswerLabel = "D", IsCorrect = correct == "D" });
                        }
                    }
                }
                
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã import {count} câu hỏi thành công!";
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "Lỗi import: " + ex.Message);
                ViewBag.LessonId = lessonId;
                return View();
            }

            return RedirectToAction("ManageQuestions", new { lessonId = lessonId });
        }

        // API: Get subjects that teacher is assigned to teach in a specific class
        [HttpGet]
        public async Task<IActionResult> GetSubjectsByClass(int classRoomId)
        {
            if (!IsTeacher())
                return Unauthorized();

            var userId = GetCurrentUserId();
            
            // Get subjects that this teacher is assigned to teach in this specific class
            var subjects = await _context.TeacherAssignments
                .Where(ta => ta.TeacherId == userId && ta.ClassRoomId == classRoomId)
                .Select(ta => ta.Subject)
                .Distinct()
                .ToListAsync();

            // Map subject codes to display names with emojis
            var subjectList = subjects.Select(s => new
            {
                Value = s,
                Text = GetSubjectDisplayName(s)
            }).ToList();

            return Json(subjectList);
        }

        // Helper: Get subject display name with emoji
        private string GetSubjectDisplayName(string subjectCode)
        {
            return subjectCode switch
            {
                "Toan" => "🔢 Toán",
                "TiengViet" => "📚 Tiếng Việt",
                "TiengAnh" => "🌎 Tiếng Anh",
                "TNXH" => "🌿 TN & XH",
                "LichSu" => "📜 Lịch Sử",
                "DiaLy" => "🗺️ Địa Lý",
                "KhoaHoc" => "🔬 Khoa Học",
                "DaoDuc" => "⭐ Đạo Đức",
                "AmNhac" => "🎵 Âm Nhạc",
                "MyThuat" => "🎨 Mỹ Thuật",
                "TheDuc" => "⚽ Thể Dục",
                "TinHoc" => "💻 Tin Học",
                _ => subjectCode
            };
        }
    }
}
