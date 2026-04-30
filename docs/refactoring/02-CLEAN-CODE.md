# 🧹 Giai Đoạn 2: Clean Code - Làm Sạch & Chuẩn Hóa Code

> **Mức độ ưu tiên**: 🟠 High
> **Ước lượng**: Trung bình (2-3 ngày)
> **Phụ thuộc**: Nên làm sau Architecture (Giai đoạn 1)

---

## 📋 Tóm Tắt Vấn Đề

Code hiện tại chứa nhiều "code smell" ảnh hưởng đến khả năng đọc, bảo trì và mở rộng. Các vấn đề chính:

1. **Debug code để lại trong production** (`System.Diagnostics.Debug.WriteLine`, `Console.WriteLine`)
2. **Code trùng lặp** (DRY violation)
3. **Magic numbers/strings** rải rác
4. **Thiếu error handling pattern thống nhất**
5. **Comment TODO/TEMP** chưa xử lý
6. **Coding convention không nhất quán**

---

## 📝 Danh Sách Task

### C1. Xóa Debug Code & TODO Comments

> [!WARNING]
> Debug code đang chạy trong production tạo rủi ro lộ thông tin nhạy cảm

**TeacherController.cs** - Dòng 104-167:
```csharp
// ❌ XÓA: Debug code
System.Diagnostics.Debug.WriteLine($"[DEBUG] CreateLesson POST - Subject: ...");
Console.WriteLine($"[DEBUG] CreateLesson POST - Subject: ...");
Console.WriteLine($"[ERROR] SaveChanges exception: {ex.Message}");
Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
Console.WriteLine($"[ERROR] InnerException: {ex.InnerException?.Message}");
```

**TeacherController.cs** - Dòng 98:
```csharp
// ❌ XÓA: ValidateAntiForgeryToken bị disable
// [ValidateAntiForgeryToken] // TODO: Temporarily disabled for debugging
```
→ **Phải bật lại** `[ValidateAntiForgeryToken]`

**Program.cs** - Dòng 28-33:
```csharp
// ❌ XÓA: Developer Exception Page luôn bật
// TEMP: Always show detailed errors for debugging
app.UseDeveloperExceptionPage();
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//}
```
→ **Phải sửa lại** để chỉ hiện Developer Exception Page trong Development mode

---

### C2. Giải Quyết Code Trùng Lặp (DRY)

#### C2.1 Hàm kiểm tra Role lặp lại
| Controller | Method | Dòng |
|-----------|--------|------|
| TeacherController | `IsTeacher()` | 20-24 |
| TeacherController | `GetCurrentUserId()` | 26-29 |
| ClassController | `IsAdmin()` | 20-24 |
| ClassController | `IsTeacher()` | 26-30 |
| ClassController | `GetCurrentUserId()` | 32-35 |
| StudentController | `IsStudent()` | 18-22 |
| StudentController | `GetCurrentUserId()` | 24-27 |

**Giải pháp**: Tạo `BaseController` hoặc dùng Authorization Filter (xem Giai đoạn 1)

```csharp
public abstract class BaseController : Controller
{
    protected int GetCurrentUserId() =>
        HttpContext.Session.GetInt32(SessionKeys.UserId) ?? 0;
    
    protected string? GetCurrentUserRole() =>
        HttpContext.Session.GetString(SessionKeys.UserRole);
    
    protected bool IsInRole(string role) =>
        GetCurrentUserRole() == role;
}
```

#### C2.2 `GetSubjectDisplayName()` trùng lặp với `SubjectHelper`

**TeacherController.cs** dòng 646-664:
```csharp
// ❌ Trùng lặp với SubjectHelper.cs
private string GetSubjectDisplayName(string subjectCode)
{
    return subjectCode switch
    {
        "Toan" => "🔢 Toán",
        // ... 12 cases
    };
}
```

**SubjectHelper.cs** đã có method `GetName()` và `GetIcon()`.

**Giải pháp**: Xóa `GetSubjectDisplayName()` trong TeacherController, thay bằng:
```csharp
var subjectList = subjects.Select(s => new
{
    Value = s,
    Text = $"{SubjectHelper.GetIcon(s)} {SubjectHelper.GetName(s)}"
}).ToList();
```

#### C2.3 ViewBag.ClassRooms trùng lặp

**TeacherController.cs** - Code load ClassRooms lặp lại 2 lần (dòng 84-91 và dòng 116-121):
```csharp
// ❌ Code lặp 2 nơi
var assignedClasses = await _context.TeacherAssignments
    .Include(ta => ta.ClassRoom)
    .Where(ta => ta.TeacherId == userId)
    .Select(ta => new { ta.ClassRoomId, ta.ClassRoom.ClassName })
    .Distinct()
    .ToListAsync();
ViewBag.ClassRooms = new SelectList(assignedClasses, "ClassRoomId", "ClassName");
```

**Giải pháp**: Extract thành private method hoặc đưa vào Service

#### C2.4 ViewBag.TeacherList trùng lặp

**ClassController.cs** - Code load TeacherList lặp lại 4 lần (dòng 106, 122, 135, 170):
```csharp
// ❌ Lặp lại 4 nơi
ViewBag.TeacherList = new SelectList(
    _context.Users.Where(u => u.Role == "Teacher"), "Id", "FullName");
```

**Giải pháp**: Extract thành private method

---

### C3. Chuẩn Hóa Error Handling

#### C3.1 Pattern xử lý lỗi không nhất quán

| Controller | Method | Cách xử lý lỗi |
|-----------|--------|----------------|
| TeacherController | CreateLesson | try-catch + TempData["ErrorMessage"] |
| TeacherController | EditLesson | Không có try-catch |
| TeacherController | DeleteLesson | Không có try-catch |
| ClassController | Edit | try-catch DbUpdateConcurrencyException |
| StudentController | SubmitQuiz | Không có try-catch |

**Giải pháp**: Tạo pattern thống nhất

```csharp
// ✅ Thống nhất: Global Exception Handler + ILogger
public class GlobalExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            // Redirect to error page
        }
    }
}
```

---

### C4. Sửa Code Style & Convention

#### C4.1 Inconsistent `using` declarations
- `ClassController.cs` dùng `using var stream = new MemoryStream();` (C# 8 style)
- `TeacherController.cs` dùng `using (var stream = file.OpenReadStream())` (classic style)

→ **Chuẩn hóa**: Dùng C# 8+ `using` declaration cho tất cả

#### C4.2 Inconsistent line endings
- Một số file dùng `\r\n` (Windows CRLF)
- Một số file dùng `\n` (Unix LF)

→ **Chuẩn hóa**: Dùng `.editorconfig` đã có, đảm bảo tất cả file dùng cùng line ending

#### C4.3 Inconsistent null handling
```csharp
// ❌ Kiểu 1: ?? operator
return HttpContext.Session.GetInt32("UserId") ?? 0;  // Trả về 0 nếu null → BUG: 0 không phải userId hợp lệ

// ❌ Kiểu 2: Check null rồi dùng
var userId = HttpContext.Session.GetInt32("UserId");
if (userId != null) { ... }
```

→ **Chuẩn hóa**: Luôn check null, không dùng `?? 0` (vì 0 là giá trị nguy hiểm)

---

### C5. Xóa Unused Imports & Dead Code

#### C5.1 Unused imports
- `TeacherController.cs` dòng 6: `using System.IO;` → Có thể không cần (implicit usings)
- Kiểm tra toàn bộ file cho unused imports

#### C5.2 Dead code
- `StudentController.cs` dòng 46: `SubmitQuizViewModel` được khai báo nhưng không được sử dụng trong SubmitQuiz action (dùng Dictionary thay vì ViewModel)
- `TeacherController.cs` dòng 44-47: `assignedClassIds` được query nhưng không bao giờ sử dụng trong `Index()`

```csharp
// ❌ DEAD CODE: Query nhưng không dùng
var assignedClassIds = await _context.TeacherAssignments
    .Where(ta => ta.TeacherId == userId)
    .Select(ta => ta.ClassRoomId)
    .ToListAsync();
```

---

### C6. Cải Thiện Naming Convention

| Hiện tại | Vấn đề | Đề xuất |
|----------|--------|---------|
| `User.cs` dùng cho cả Student, Teacher, Admin | Ambiguous | Cân nhắc rename hoặc thêm comment rõ ràng |
| `ClassRoom` vs `Class` | Inconsistent in views | Giữ `ClassRoom` nhưng thống nhất tên biến |
| `CreatedByUserId` | Tên dài | OK, giữ nguyên vì rõ ràng |
| Typo: `"Fore Teacher"` (User.cs dòng 42) | Typo | Sửa thành `"For Teacher"` |
| `GetCell(params string[])` (local function) | Unnamed inline function | Extract ra hoặc thêm XML doc |

---

### C7. Thêm XML Documentation

Thêm XML doc cho tất cả:
- Public classes
- Public methods trong Services
- Complex business logic

```csharp
/// <summary>
/// Xử lý việc giáo viên tạo bài học mới cho lớp được phân công
/// </summary>
/// <param name="model">Thông tin bài học cần tạo</param>
/// <returns>ID của bài học vừa tạo</returns>
public async Task<int> CreateLessonAsync(CreateLessonViewModel model, int teacherId)
```

---

## 📍 Các File Bị Ảnh Hưởng

| File | Hành động | Chi tiết |
|------|-----------|----------|
| `Controllers/TeacherController.cs` | MODIFY | Xóa debug code, fix trùng lặp |
| `Controllers/ClassController.cs` | MODIFY | Fix trùng lặp, standardize |
| `Controllers/StudentController.cs` | MODIFY | Fix trùng lặp, standardize |
| `Controllers/AccountController.cs` | MODIFY | Minor cleanup |
| `Program.cs` | MODIFY | Fix Developer Exception Page |
| `Models/User.cs` | MODIFY | Fix typo |
| `Controllers/BaseController.cs` | NEW | Base class for common methods |

---

## ✅ Tiêu Chí Hoàn Thành

- [ ] Không còn `System.Diagnostics.Debug.WriteLine` hoặc `Console.WriteLine` debug
- [ ] Không còn TODO/TEMP comments chưa xử lý
- [ ] `[ValidateAntiForgeryToken]` được bật lại
- [ ] Developer Exception Page chỉ hiện trong Development
- [ ] Không còn code trùng lặp (DRY)
- [ ] Naming convention thống nhất
- [ ] Error handling pattern thống nhất
- [ ] XML Documentation cho public APIs
- [ ] Không còn unused imports hoặc dead code
