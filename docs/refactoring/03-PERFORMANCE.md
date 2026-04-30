# ⚡ Giai Đoạn 3: Performance - Tối Ưu Hiệu Suất

> **Mức độ ưu tiên**: 🟡 Medium
> **Ước lượng**: Trung bình (2-3 ngày)
> **Phụ thuộc**: Nên làm sau Clean Code (Giai đoạn 2)

---

## 📋 Tóm Tắt Vấn Đề

Dự án hiện tại có nhiều vấn đề hiệu suất liên quan đến:

1. **N+1 Query Problem** - Gọi SaveChanges trong vòng lặp
2. **Over-fetching** - Include quá nhiều navigation properties không cần thiết
3. **Thiếu AsNoTracking** - Cho các query chỉ đọc
4. **Thiếu Pagination** ở một số endpoint
5. **Thiếu Caching** cho dữ liệu ít thay đổi

---

## 📝 Danh Sách Task

### P1. Fix N+1 Query Problem (🔴 Nghiêm trọng)

#### P1.1 `UploadQuestions` - SaveChanges trong vòng lặp

**TeacherController.cs** dòng 549-606:
```csharp
// ❌ BAD: Gọi SaveChanges() cho MỖI câu hỏi trong vòng lặp
foreach (System.Data.DataRow row in table.Rows)
{
    var question = new Question { ... };
    _context.Questions.Add(question);
    await _context.SaveChangesAsync();      // ← HIT DB mỗi lần!

    _context.Answers.Add(new Answer { ... });  // 4 answers
    _context.Answers.Add(new Answer { ... });
    _context.Answers.Add(new Answer { ... });
    _context.Answers.Add(new Answer { ... });
    // ← KHÔNG SaveChanges ở đây, chỉ save bên ngoài loop
}
await _context.SaveChangesAsync();  // ← Chỉ save answers, không save questions
```

**Vấn đề**: Nếu import 100 câu hỏi → 100 lần round-trip đến database!

**Giải pháp**:
```csharp
// ✅ GOOD: Batch insert, save 1 lần duy nhất
var questions = new List<Question>();
var allAnswers = new List<Answer>();

foreach (System.Data.DataRow row in table.Rows)
{
    var question = new Question { ... };
    questions.Add(question);
}

_context.Questions.AddRange(questions);
await _context.SaveChangesAsync(); // 1 lần cho tất cả questions

// Giờ question.Id đã có giá trị
foreach (var q in questions)
{
    allAnswers.AddRange(CreateAnswersForQuestion(q));
}

_context.Answers.AddRange(allAnswers);
await _context.SaveChangesAsync(); // 1 lần cho tất cả answers
```

**Tác động**: Giảm từ `N*2` xuống `2` round-trips (N = số câu hỏi)

#### P1.2 `ImportStudents` - SaveChanges cuối loop nhưng check username trong loop

**ClassController.cs** dòng 466-536:
```csharp
// ❌ BAD: Query EXISTS cho MỖI row
foreach (var row in rows)
{
    var existingUser = await _context.Users
        .AnyAsync(u => u.Username.ToLower() == username.ToLower()); // ← HIT DB mỗi lần!
    
    _context.Users.Add(student);
}
await _context.SaveChangesAsync(); // ← Đúng: Save 1 lần cuối
```

**Giải pháp**: Pre-load tất cả usernames vào HashSet
```csharp
// ✅ GOOD: Load 1 lần, check in-memory
var existingUsernames = await _context.Users
    .Select(u => u.Username.ToLower())
    .ToHashSetAsync();

foreach (var row in rows)
{
    if (existingUsernames.Contains(username.ToLower()))
    {
        // error...
        continue;
    }
    existingUsernames.Add(username.ToLower()); // Track new additions
    _context.Users.Add(student);
}
await _context.SaveChangesAsync();
```

#### P1.3 `SubmitQuiz` - SaveChanges 2 lần liên tiếp

**StudentController.cs** dòng 210-219:
```csharp
// ❌ BAD: 2 round-trips
_context.TestResults.Add(testResult);
await _context.SaveChangesAsync();        // Round-trip 1

foreach (var detail in testDetails)
    detail.TestResultId = testResult.Id;
_context.TestDetails.AddRange(testDetails);
await _context.SaveChangesAsync();        // Round-trip 2
```

**Giải pháp**: Dùng navigation property, save 1 lần
```csharp
// ✅ GOOD: 1 round-trip
testResult.Details = testDetails;  // EF Core tự gán FK
_context.TestResults.Add(testResult);
await _context.SaveChangesAsync();        // 1 lần duy nhất
```

---

### P2. Thêm AsNoTracking cho Read-Only Queries

EF Core tracking entities tốn bộ nhớ và CPU. Các query chỉ đọc nên dùng `AsNoTracking()`.

| File | Method | Dòng | Cần AsNoTracking? |
|------|--------|------|-------------------|
| TeacherController | `Index()` | 49-54 | ✅ Có (chỉ hiển thị) |
| TeacherController | `MyClasses()` | 66-70 | ✅ Có |
| StudentController | `Index()` | 38-55 | ✅ Có |
| StudentController | `TakeQuiz()` | 111-114 | ✅ Có |
| StudentController | `Result()` | 232-234 | ✅ Có |
| StudentController | `ReviewQuiz()` | 277-282 | ✅ Có |
| StudentController | `History()` | 337-341 | ✅ Có |
| ClassController | `Index()` | 66-91 | ✅ Có |
| ClassController | `Details()` | 183-188 | ✅ Có |

**Ví dụ**:
```csharp
// ❌ TRƯỚC
var lessons = await _context.Lessons
    .Where(l => l.CreatedByUserId == userId)
    .Include(l => l.Questions)
    .ToListAsync();

// ✅ SAU
var lessons = await _context.Lessons
    .AsNoTracking()
    .Where(l => l.CreatedByUserId == userId)
    .Include(l => l.Questions)
    .ToListAsync();
```

---

### P3. Tối Ưu Include (Tránh Over-fetching)

#### P3.1 `StudentController.Index()` - Load quá nhiều
```csharp
// ❌ BAD: Load toàn bộ Questions chỉ để đếm Count
var lessons = await _context.Lessons
    .Where(l => l.IsActive && l.ClassRoomId == classRoomId)
    .Include(l => l.Questions)        // ← Load TOÀN BỘ questions chỉ để đếm!
    .Include(l => l.CreatedByUser)    // ← Chỉ cần FullName
    .ToListAsync();
```

**Giải pháp**: Dùng Projection
```csharp
// ✅ GOOD: Chỉ lấy dữ liệu cần thiết
var lessons = await _context.Lessons
    .AsNoTracking()
    .Where(l => l.IsActive && l.ClassRoomId == classRoomId)
    .Select(l => new LessonCardViewModel
    {
        Id = l.Id,
        Title = l.Title,
        Description = l.Description,
        Subject = l.Subject,
        LessonNumber = l.LessonNumber,
        QuestionCount = l.Questions.Count,  // ← SQL COUNT, không load entities
    })
    .ToListAsync();
```

#### P3.2 `TeacherController.DeleteLesson()` - Include chain quá dài
```csharp
// ❌ BAD: 5 Include/ThenInclude chỉ để xóa
var lesson = await _context.Lessons
    .Include(l => l.Questions)
        .ThenInclude(q => q.Answers)
    .Include(l => l.Questions)
        .ThenInclude(q => q.TestDetails)
    .Include(l => l.TestResults)
        .ThenInclude(tr => tr.Details)
    .FirstOrDefaultAsync(l => l.Id == id);
```

**Giải pháp**: Dùng `ExecuteDeleteAsync` (EF Core 7+) hoặc raw SQL
```csharp
// ✅ GOOD: Cascade delete đã cấu hình trong DbContext
// Chỉ cần xóa TestDetails trước (vì Restrict), sau đó cascade lo phần còn lại
await _context.TestDetails
    .Where(td => td.TestResult.LessonId == id)
    .ExecuteDeleteAsync();

await _context.TestResults
    .Where(tr => tr.LessonId == id)
    .ExecuteDeleteAsync();

await _context.Lessons
    .Where(l => l.Id == id && l.CreatedByUserId == teacherId)
    .ExecuteDeleteAsync();
```

---

### P4. Thêm Pagination Cho Các Endpoint Thiếu

| Endpoint | Hiện tại | Cần thêm? |
|----------|----------|-----------|
| ClassController.Index | ✅ Đã có (pageSize=9) | OK |
| TeacherController.Index | ❌ Load ALL lessons | ✅ Cần thêm |
| StudentController.Index | ❌ Load ALL lessons | ✅ Cần thêm |
| StudentController.History | ❌ Load ALL results | ✅ Cần thêm |

---

### P5. Caching Cho Dữ Liệu Ít Thay Đổi

| Dữ liệu | Tần suất thay đổi | Cache Strategy |
|----------|-------------------|----------------|
| `SubjectHelper.Subjects` | Static | ✅ Đã là static Dictionary |
| Danh sách giáo viên (cho dropdown) | Rất ít | Memory Cache 5 phút |
| Danh sách lớp học | Ít | Memory Cache 5 phút |

```csharp
// Program.cs
builder.Services.AddMemoryCache();

// Usage
private async Task<List<User>> GetTeachersAsync()
{
    return await _cache.GetOrCreateAsync("teachers_list", async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Role == "Teacher")
            .ToListAsync();
    });
}
```

---

### P6. Tối Ưu ViewBag → Strongly-Typed ViewModel

ViewBag là dynamic, không có type-checking. Nên chuyển sang ViewModel.

```csharp
// ❌ BAD: ViewBag
ViewBag.UserName = HttpContext.Session.GetString("UserName");
ViewBag.ClassName = student?.ClassRoom?.ClassName;
ViewBag.CurrentPage = page;
ViewBag.TotalPages = totalPages;

// ✅ GOOD: ViewModel
public class ClassListViewModel
{
    public List<ClassRoom> Classes { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public int? FilterGrade { get; set; }
}
```

---

## 📍 Các File Bị Ảnh Hưởng

| File | Hành động | Ưu tiên |
|------|-----------|---------|
| `Controllers/TeacherController.cs` | MODIFY | 🔴 P1.1 (N+1) |
| `Controllers/ClassController.cs` | MODIFY | 🔴 P1.2 (N+1) |
| `Controllers/StudentController.cs` | MODIFY | 🟠 P1.3, P2, P3 |
| `Program.cs` | MODIFY | 🟡 P5 (MemoryCache) |
| `ViewModels/*` | MODIFY | 🟢 P6 |

---

## 📊 Dự Kiến Cải Thiện

| Metric | Trước | Sau |
|--------|-------|-----|
| Import 100 câu hỏi | ~200 DB round-trips | ~2 DB round-trips |
| Import 50 students | ~50 EXISTS queries | ~1 SELECT query |
| Submit Quiz | 2 round-trips | 1 round-trip |
| Dashboard load | Full entity tracking | No tracking (50% less memory) |

---

## ✅ Tiêu Chí Hoàn Thành

- [ ] Không còn `SaveChangesAsync()` trong vòng lặp
- [ ] Tất cả read-only queries dùng `AsNoTracking()`
- [ ] Không còn over-fetching (dùng Projection khi có thể)
- [ ] Tất cả list endpoints có pagination
- [ ] MemoryCache cho dropdown data
- [ ] Chuyển ViewBag sang ViewModel cho các view phức tạp
- [ ] App hoạt động đúng, performance test cải thiện rõ rệt
