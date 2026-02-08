using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HistoryGeoQuiz_PrimarySchool.Data;
using HistoryGeoQuiz_PrimarySchool.Models;

namespace HistoryGeoQuiz_PrimarySchool.Controllers
{
    public class ClassController : Controller
    {
        private readonly AppDbContext _context;

        public ClassController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role == "Admin";
        }

        // GET: Class
        public async Task<IActionResult> Index(int? grade, int page = 1)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            
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
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.TeacherList = new SelectList(_context.Users.Where(u => u.Role == "Teacher"), "Id", "FullName");
            return View();
        }

        // POST: Class/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClassName,Grade,HomeroomTeacherId")] ClassRoom classRoom)
        {
            if (ModelState.IsValid)
            {
                classRoom.CreatedAt = DateTime.UtcNow;
                _context.Add(classRoom);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.TeacherList = new SelectList(_context.Users.Where(u => u.Role == "Teacher"), "Id", "FullName", classRoom.HomeroomTeacherId);
            return View(classRoom);
        }

        // GET: Class/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var classRoom = await _context.ClassRooms.FindAsync(id);
            if (classRoom == null) return NotFound();

            ViewBag.TeacherList = new SelectList(_context.Users.Where(u => u.Role == "Teacher"), "Id", "FullName", classRoom.HomeroomTeacherId);
            return View(classRoom);
        }

        // POST: Class/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClassName,Grade,HomeroomTeacherId,CreatedAt")] ClassRoom classRoom)
        {
            if (id != classRoom.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
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
            ViewBag.TeacherList = new SelectList(_context.Users.Where(u => u.Role == "Teacher"), "Id", "FullName", classRoom.HomeroomTeacherId);
            return View(classRoom);
        }

        // GET: Class/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var classRoom = await _context.ClassRooms
                .Include(c => c.HomeroomTeacher)
                .Include(c => c.Students)
                .Include(c => c.TeacherAssignments)
                    .ThenInclude(ta => ta.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (classRoom == null) return NotFound();

            // Populate Teachers list for the Assignment Dropdown
            // Exclude teachers already assigned to avoid confusion? Or just list all.
            var teachers = await _context.Users
                .Where(u => u.Role == "Teacher")
                .Select(u => new { u.Id, u.FullName })
                .ToListAsync();
            
            ViewBag.Teachers = new SelectList(teachers, "Id", "FullName");

            return View(classRoom);
        }

        // POST: Class/AssignTeacher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTeacher(int classRoomId, int teacherId, string subject)
        {
            var exists = await _context.TeacherAssignments
                .AnyAsync(ta => ta.ClassRoomId == classRoomId && ta.TeacherId == teacherId && ta.Subject == subject);

            if (!exists)
            {
                var assignment = new TeacherAssignment
                {
                    ClassRoomId = classRoomId,
                    TeacherId = teacherId,
                    Subject = subject,
                    AssignedDate = DateTime.UtcNow
                };
                _context.Add(assignment);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Details), new { id = classRoomId });
        }

        // POST: Class/RemoveAssignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAssignment(int assignmentId)
        {
            var assignment = await _context.TeacherAssignments.FindAsync(assignmentId);
            if (assignment != null)
            {
                int classId = assignment.ClassRoomId;
                _context.TeacherAssignments.Remove(assignment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = classId });
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ClassRoomExists(int id)
        {
            return _context.ClassRooms.Any(e => e.Id == id);
        }
    }
}
