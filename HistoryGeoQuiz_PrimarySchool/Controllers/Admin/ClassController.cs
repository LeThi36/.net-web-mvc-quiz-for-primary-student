using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HistoryGeoQuiz_PrimarySchool.Data;
using HistoryGeoQuiz_PrimarySchool.Models;
using HistoryGeoQuiz_PrimarySchool.ViewModels;
using HistoryGeoQuiz_PrimarySchool.ViewModels.Admin;
using HistoryGeoQuiz_PrimarySchool.Constants;
using HistoryGeoQuiz_PrimarySchool.Enums;
using HistoryGeoQuiz_PrimarySchool.Helpers;
using ClosedXML.Excel;
using System.Web;

namespace HistoryGeoQuiz_PrimarySchool.Controllers
{
    public class ClassController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ClassController> _logger;

        // [S8.1] Max file size for Excel uploads: 5MB
        private const long MaxUploadFileSize = 5 * 1024 * 1024;

        public ClassController(AppDbContext context, ILogger<ClassController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private void PopulateTeacherList(Guid? selectedId = null)
        {
            ViewBag.TeacherList = new SelectList(_context.Users.Where(u => u.Role == UserRole.Teacher), "Id", "FullName", selectedId);
        }


        private async Task<bool> CanManageClass(Guid classId)
        {
            if (IsInRole(UserRole.Admin)) return true;
            if (!IsInRole(UserRole.Teacher)) return false;

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return false;

            // Teacher can manage if they are homeroom teacher or assigned to the class
            var canManage = await _context.ClassRooms
                .AnyAsync(c => c.Id == classId && c.HomeroomTeacherId == userId);
            
            if (!canManage)
            {
                canManage = await _context.TeacherAssignments
                    .AnyAsync(ta => ta.ClassRoomId == classId && ta.TeacherId == userId);
            }

            return canManage;
        }

        // GET: Class
        public async Task<IActionResult> Index(int? grade, int page = 1)
        {
            if (!IsInRole(UserRole.Admin)) return RedirectToAction("Login", "Account");
            
            const int pageSize = 9; // 9 items per page
            
            // Start with base query
            var query = _context.ClassRooms
                .Include(c => c.HomeroomTeacher)
                .Include(c => c.Students)
                .AsQueryable();
            
            // Apply grade filter if specified
            if (grade.HasValue && grade.Value >= 1 && grade.Value <= 5)
            {
                query = query.Where(c => c.Grade == grade.Value);
                ViewBag.CurrentGrade = grade.Value;
            }
            
            // Get total count for pagination
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            // Ensure page is within valid range
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;
            
            // Get paginated data
            var classes = await query
                .OrderBy(c => c.Grade).ThenBy(c => c.ClassName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            // Pass pagination metadata to view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            
            return View(classes);
        }

        // GET: Class/Create
        public IActionResult Create()
        {
            if (!IsInRole(UserRole.Admin)) return RedirectToAction("Login", "Account");
            PopulateTeacherList();
            return View();
        }

        // POST: Class/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClassName,Grade,HomeroomTeacherId")] ClassRoom classRoom)
        {
            if (ModelState.IsValid)
            {
                classRoom.CreatedAt = DateTimeHelper.GetVietnamTime();
                _context.Add(classRoom);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateTeacherList(classRoom.HomeroomTeacherId);
            return View(classRoom);
        }

        // GET: Class/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (!IsInRole(UserRole.Admin)) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var classRoom = await _context.ClassRooms.FindAsync(id);
            if (classRoom == null) return NotFound();

            PopulateTeacherList(classRoom.HomeroomTeacherId);
            return View(classRoom);
        }

        // POST: Class/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,ClassName,Grade,HomeroomTeacherId,CreatedAt")] ClassRoom classRoom)
        {
            if (id != classRoom.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // No need for SpecifyKind due to legacy behavior enablement
                    _context.Update(classRoom);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassRoomExists(classRoom.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateTeacherList(classRoom.HomeroomTeacherId);
            return View(classRoom);
        }

        // GET: Class/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();
            
            // Allow both Admin and Teachers assigned to this class
            if (!await CanManageClass(id.Value)) 
                return RedirectToAction("Login", "Account");

            var classRoom = await _context.ClassRooms
                .Include(c => c.HomeroomTeacher)
                .Include(c => c.Students)
                .Include(c => c.TeacherAssignments)
                    .ThenInclude(ta => ta.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (classRoom == null) return NotFound();

            // Populate Teachers list for the Assignment Dropdown (Admin only)
            ViewBag.IsAdmin = IsInRole(UserRole.Admin);
            if (IsInRole(UserRole.Admin))
            {
                var teachers = await _context.Users
                    .Where(u => u.Role == UserRole.Teacher)
                    .Select(u => new { u.Id, u.FullName })
                    .ToListAsync();
                
                ViewBag.Teachers = new SelectList(teachers, "Id", "FullName");
            }

            return View(classRoom);
        }

        // POST: Class/AssignTeacher
        // [S6] FIX: Added IsInRole(UserRole.Admin) authorization check
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTeacher(Guid classRoomId, Guid teacherId, string subject)
        {
            if (!IsInRole(UserRole.Admin)) return RedirectToAction("Login", "Account");

            var exists = await _context.TeacherAssignments
                .AnyAsync(ta => ta.ClassRoomId == classRoomId && ta.TeacherId == teacherId && ta.Subject == subject);

            if (!exists)
            {
                var assignment = new TeacherAssignment
                {
                    ClassRoomId = classRoomId,
                    TeacherId = teacherId,
                    Subject = subject,
                    AssignedDate = DateTimeHelper.GetVietnamTime()
                };
                _context.Add(assignment);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Teacher {TeacherId} assigned to class {ClassId} for subject {Subject}", teacherId, classRoomId, subject);
            }
            
            return RedirectToAction(nameof(Details), new { id = classRoomId });
        }

        // POST: Class/RemoveAssignment
        // [S6] FIX: Added IsInRole(UserRole.Admin) authorization check
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAssignment(Guid assignmentId)
        {
            if (!IsInRole(UserRole.Admin)) return RedirectToAction("Login", "Account");

            var assignment = await _context.TeacherAssignments.FindAsync(assignmentId);
            if (assignment != null)
            {
                Guid classId = assignment.ClassRoomId;
                _context.TeacherAssignments.Remove(assignment);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Assignment {AssignmentId} removed from class {ClassId}", assignmentId, classId);
                return RedirectToAction(nameof(Details), new { id = classId });
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ClassRoomExists(Guid id)
        {
            return _context.ClassRooms.Any(e => e.Id == id);
        }

        // ==================== PHASE 1: ADD STUDENTS TO CLASS ====================

        // GET: Class/AddStudent/5
        public async Task<IActionResult> AddStudent(Guid? id)
        {
            if (id == null) return NotFound();
            if (!await CanManageClass(id.Value)) return RedirectToAction("Login", "Account");

            var classRoom = await _context.ClassRooms.FindAsync(id);
            if (classRoom == null) return NotFound();

            var viewModel = new AddStudentToClassViewModel
            {
                ClassRoomId = classRoom.Id,
                ClassName = classRoom.ClassName
            };

            return View(viewModel);
        }

        // POST: Class/AddStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(AddStudentToClassViewModel model)
        {
            if (!await CanManageClass(model.ClassRoomId))
                return RedirectToAction("Login", "Account");

            var classRoom = await _context.ClassRooms.FindAsync(model.ClassRoomId);
            if (classRoom == null) return NotFound();
            model.ClassName = classRoom.ClassName;

            if (ModelState.IsValid)
            {
                // Check if username already exists
                var existingUser = await _context.Users
                    .AnyAsync(u => u.Username.ToLower() == model.Username.ToLower());

                if (existingUser)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }

                // Create new student account
                // [S1] FIX: Hash password with BCrypt
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
                var student = new User
                {
                    FullName = model.FullName,
                    Username = model.Username,
                    Password = hashedPassword,
                    Role = UserRole.Student,
                    ClassRoomId = model.ClassRoomId,
                    CreatedAt = DateTimeHelper.GetVietnamTime()
                };

                _context.Users.Add(student);
                await _context.SaveChangesAsync();

                TempData[TempDataKeys.SuccessMessage] = $"Đã thêm học sinh {model.FullName} vào lớp {classRoom.ClassName}";
                return RedirectToAction(nameof(Details), new { id = model.ClassRoomId });
            }

            return View(model);
        }

        // POST: Class/RemoveStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveStudent(Guid classRoomId, Guid studentId)
        {
            if (!await CanManageClass(classRoomId))
                return RedirectToAction("Login", "Account");

            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == studentId && u.ClassRoomId == classRoomId);

            if (student != null)
            {
                // Just remove from class, don't delete the account
                student.ClassRoomId = null;
                await _context.SaveChangesAsync();
                TempData[TempDataKeys.SuccessMessage] = $"Đã xóa học sinh {student.FullName} khỏi lớp";
            }

            return RedirectToAction(nameof(Details), new { id = classRoomId });
        }

        // GET: Class/AssignExistingStudent/5
        public async Task<IActionResult> AssignExistingStudent(Guid? id)
        {
            if (id == null) return NotFound();
            if (!await CanManageClass(id.Value)) return RedirectToAction("Login", "Account");

            var classRoom = await _context.ClassRooms.FindAsync(id);
            if (classRoom == null) return NotFound();

            // Get students without a class
            var availableStudents = await _context.Users
                .Where(u => u.Role == UserRole.Student && u.ClassRoomId == null)
                .OrderBy(u => u.FullName)
                .Select(u => new StudentSelectItem
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Username = u.Username
                })
                .ToListAsync();

            var viewModel = new AssignExistingStudentViewModel
            {
                ClassRoomId = classRoom.Id,
                ClassName = classRoom.ClassName,
                AvailableStudents = availableStudents
            };

            return View(viewModel);
        }

        // POST: Class/AssignExistingStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignExistingStudent(AssignExistingStudentViewModel model)
        {
            if (!await CanManageClass(model.ClassRoomId))
                return RedirectToAction("Login", "Account");

            var classRoom = await _context.ClassRooms.FindAsync(model.ClassRoomId);
            if (classRoom == null) return NotFound();

            // Re-populate students for validation errors
            model.ClassName = classRoom.ClassName;
            model.AvailableStudents = await _context.Users
                .Where(u => u.Role == UserRole.Student && u.ClassRoomId == null)
                .OrderBy(u => u.FullName)
                .Select(u => new StudentSelectItem
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Username = u.Username
                })
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Find the student
            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == model.StudentId && u.Role == UserRole.Student && u.ClassRoomId == null);

            if (student == null)
            {
                ModelState.AddModelError("StudentId", "Học sinh không tồn tại hoặc đã thuộc lớp khác");
                return View(model);
            }

            // Assign to class
            student.ClassRoomId = model.ClassRoomId;
            await _context.SaveChangesAsync();

            TempData[TempDataKeys.SuccessMessage] = $"Đã thêm học sinh {student.FullName} vào lớp {classRoom.ClassName}";
            return RedirectToAction(nameof(Details), new { id = model.ClassRoomId });
        }

        // GET: Class/ImportStudents/5
        public async Task<IActionResult> ImportStudents(Guid? id)
        {
            if (id == null) return NotFound();
            if (!await CanManageClass(id.Value)) return RedirectToAction("Login", "Account");

            var classRoom = await _context.ClassRooms.FindAsync(id);
            if (classRoom == null) return NotFound();

            var viewModel = new ImportStudentsViewModel
            {
                ClassRoomId = classRoom.Id,
                ClassName = classRoom.ClassName
            };

            return View(viewModel);
        }

        // POST: Class/ImportStudents
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportStudents(ImportStudentsViewModel model)
        {
            if (!await CanManageClass(model.ClassRoomId))
                return RedirectToAction("Login", "Account");

            var classRoom = await _context.ClassRooms.FindAsync(model.ClassRoomId);
            if (classRoom == null) return NotFound();

            var result = new ImportStudentsResultViewModel
            {
                ClassRoomId = model.ClassRoomId,
                ClassName = classRoom.ClassName
            };

            if (model.ExcelFile == null || model.ExcelFile.Length == 0)
            {
                ModelState.AddModelError("ExcelFile", "Vui lòng chọn file Excel");
                return View(model);
            }

            // [S8.1] FIX: Validate file size to prevent DoS
            if (model.ExcelFile.Length > MaxUploadFileSize)
            {
                ModelState.AddModelError("ExcelFile", "File không được vượt quá 5MB");
                return View(model);
            }

            // Check file extension
            var extension = Path.GetExtension(model.ExcelFile.FileName).ToLower();
            if (extension != ".xlsx")
            {
                ModelState.AddModelError("ExcelFile", "Chỉ hỗ trợ file Excel .xlsx");
                return View(model);
            }

            try
            {
                using var stream = new MemoryStream();
                await model.ExcelFile.CopyToAsync(stream);
                stream.Position = 0;

                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed().Skip(1); // Skip header row

                // P1.2 FIX: Pre-load all usernames into HashSet (1 DB query instead of N)
                var existingUsernames = (await _context.Users
                    .Select(u => u.Username.ToLower())
                    .ToListAsync())
                    .ToHashSet();

                foreach (var row in rows)
                {
                    result.TotalRows++;
                    var rowNum = row.RowNumber();

                    var fullName = row.Cell(1).GetString()?.Trim();
                    var username = row.Cell(2).GetString()?.Trim();
                    var password = row.Cell(3).GetString()?.Trim();

                    // Validate row data
                    if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    {
                        result.Errors.Add(new ImportStudentError
                        {
                            RowNumber = rowNum,
                            FullName = fullName,
                            Username = username,
                            ErrorMessage = "Thiếu thông tin (Họ tên, Tên đăng nhập hoặc Mật khẩu)"
                        });
                        result.ErrorCount++;
                        continue;
                    }

                    // Check if username already exists (in-memory lookup)
                    if (existingUsernames.Contains(username.ToLower()))
                    {
                        result.Errors.Add(new ImportStudentError
                        {
                            RowNumber = rowNum,
                            FullName = fullName,
                            Username = username,
                            ErrorMessage = "Tên đăng nhập đã tồn tại"
                        });
                        result.ErrorCount++;
                        continue;
                    }

                    // Track new username to detect duplicates within the same import file
                    existingUsernames.Add(username.ToLower());

                    // Create student account
                    // [S1] FIX: Hash password with BCrypt
                    // [S8.2] FIX: Sanitize input from Excel
                    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                    var student = new User
                    {
                        FullName = HttpUtility.HtmlEncode(fullName),
                        Username = username, // Username kept as-is for login matching
                        Password = hashedPassword,
                        Role = UserRole.Student,
                        ClassRoomId = model.ClassRoomId,
                        CreatedAt = DateTimeHelper.GetVietnamTime()
                    };

                    _context.Users.Add(student);
                    result.SuccessfulStudents.Add(new ImportStudentSuccess
                    {
                        FullName = fullName,
                        Username = username
                    });
                    result.SuccessCount++;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // [S11] FIX: Log details server-side, show generic message to user
                _logger.LogError(ex, "Error importing students for class {ClassId}", model.ClassRoomId);
                ModelState.AddModelError("ExcelFile", "Có lỗi xảy ra khi đọc file Excel. Vui lòng kiểm tra lại định dạng file.");
                return View(model);
            }

            return View("ImportStudentsResult", result);
        }

        // GET: Class/DownloadStudentTemplate
        // [S6] FIX: Added authorization check
        public IActionResult DownloadStudentTemplate()
        {
            if (!IsInRole(UserRole.Admin) && !IsInRole(UserRole.Teacher)) return RedirectToAction("Login", "Account");
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("DanhSachHocSinh");

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
            worksheet.Cell(2, 1).Value = "Nguyễn Văn A";
            worksheet.Cell(2, 2).Value = "nguyenvana";
            worksheet.Cell(2, 3).Value = "123456";

            worksheet.Cell(3, 1).Value = "Trần Thị B";
            worksheet.Cell(3, 2).Value = "tranthib";
            worksheet.Cell(3, 3).Value = "123456";

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "MauDanhSachHocSinh.xlsx"
            );
        }

        // ==================== PHASE 2: TRANSFER STUDENTS BETWEEN CLASSES ====================

        // GET: Class/TransferStudent/5
        public async Task<IActionResult> TransferStudent(Guid? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Users
                .Include(u => u.ClassRoom)
                .FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRole.Student);

            if (student == null) return NotFound();

            // Check if user can manage the current class
            if (student.ClassRoomId.HasValue && !await CanManageClass(student.ClassRoomId.Value))
                return RedirectToAction("Login", "Account");

            // Get all classes for dropdown (exclude current class)
            var availableClasses = await _context.ClassRooms
                .Where(c => c.Id != student.ClassRoomId)
                .OrderBy(c => c.Grade)
                .ThenBy(c => c.ClassName)
                .Select(c => new ClassRoomSelectItem
                {
                    Id = c.Id,
                    DisplayName = $"Lớp {c.ClassName} - Khối {c.Grade}"
                })
                .ToListAsync();

            var viewModel = new TransferStudentViewModel
            {
                StudentId = student.Id,
                StudentName = student.FullName,
                CurrentClassRoomId = student.ClassRoomId ?? Guid.Empty,
                CurrentClassName = student.ClassRoom?.ClassName,
                AvailableClasses = availableClasses
            };

            return View(viewModel);
        }

        // POST: Class/TransferStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferStudent(TransferStudentViewModel model)
        {
            var student = await _context.Users
                .Include(u => u.ClassRoom)
                .FirstOrDefaultAsync(u => u.Id == model.StudentId && u.Role == UserRole.Student);

            if (student == null) return NotFound();

            // Re-populate classes for validation errors
            model.StudentName = student.FullName;
            model.CurrentClassName = student.ClassRoom?.ClassName;
            model.AvailableClasses = await _context.ClassRooms
                .Where(c => c.Id != student.ClassRoomId)
                .OrderBy(c => c.Grade)
                .ThenBy(c => c.ClassName)
                .Select(c => new ClassRoomSelectItem
                {
                    Id = c.Id,
                    DisplayName = $"Lớp {c.ClassName} - Khối {c.Grade}"
                })
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if target class exists
            var targetClass = await _context.ClassRooms.FindAsync(model.TargetClassRoomId);
            if (targetClass == null)
            {
                ModelState.AddModelError("TargetClassRoomId", "Lớp đích không tồn tại");
                return View(model);
            }

            // Store old class ID for redirect
            Guid? oldClassId = student.ClassRoomId;

            // Transfer student
            student.ClassRoomId = model.TargetClassRoomId;
            await _context.SaveChangesAsync();

            TempData[TempDataKeys.SuccessMessage] = $"Đã chuyển học sinh {student.FullName} sang lớp {targetClass.ClassName}";

            // Redirect to the new class details
            return RedirectToAction(nameof(Details), new { id = model.TargetClassRoomId });
        }
    }
}
