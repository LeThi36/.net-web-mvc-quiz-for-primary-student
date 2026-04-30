# 🏗️ Giai Đoạn 1: Architecture - Tái Cấu Trúc Kiến Trúc

> **Mức độ ưu tiên**: 🔴 Critical
> **Ước lượng**: Lớn (3-5 ngày)
> **Phụ thuộc**: Nên làm sau Security (Giai đoạn 5)

---

## 📋 Tóm Tắt Vấn Đề

Hiện tại dự án đang sử dụng kiến trúc **Monolithic MVC** mà không có Service Layer. Toàn bộ business logic, data access, và presentation logic đều nằm trong Controllers. Điều này gây ra:

- **Fat Controllers**: `TeacherController` (667 dòng), `ClassController` (680 dòng)
- **Không thể test**: Logic nằm trong Controller, phụ thuộc trực tiếp vào `HttpContext`
- **Code trùng lặp**: Hàm `IsTeacher()`, `GetCurrentUserId()` xuất hiện ở nhiều Controller
- **Tight coupling**: Controller truy cập trực tiếp `DbContext`

---

## 🎯 Mục Tiêu

Chuyển từ **MVC → MVC + Service Layer + Repository Pattern**

```
HIỆN TẠI:                          MỤC TIÊU:
┌─────────┐                        ┌─────────┐
│  View   │                        │  View   │
└────┬────┘                        └────┬────┘
     │                                  │
┌────▼────┐                        ┌────▼────┐
│Controller│ ◄─ Fat!               │Controller│ ◄─ Thin!
│(Logic+DB)│                       └────┬────┘
└────┬────┘                             │
     │                             ┌────▼────┐
┌────▼────┐                        │ Service │ ◄─ Business Logic
│DbContext │                       └────┬────┘
└─────────┘                             │
                                   ┌────▼────┐
                                   │   Repo  │ ◄─ Data Access
                                   └────┬────┘
                                        │
                                   ┌────▼────┐
                                   │DbContext │
                                   └─────────┘
```

---

## 📝 Danh Sách Task

### A1. Tạo cấu trúc thư mục mới

```
HistoryGeoQuiz_PrimarySchool/
├── Controllers/          ← Giữ nguyên, làm mỏng
├── Services/             ← MỚI: Business Logic
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── ILessonService.cs
│   │   ├── IQuestionService.cs
│   │   ├── IClassService.cs
│   │   ├── IStudentService.cs
│   │   └── ITestService.cs
│   ├── AuthService.cs
│   ├── LessonService.cs
│   ├── QuestionService.cs
│   ├── ClassService.cs
│   ├── StudentService.cs
│   └── TestService.cs
├── Repositories/         ← Data Access
├── Models/               ← Giữ nguyên
├── ViewModels/           ← Giữ nguyên
├── Data/                 ← Giữ nguyên
├── Helpers/              ← Giữ nguyên
├── Enums/                ← MỚI
│   ├── UserRole.cs       ← Thay magic strings "Teacher", "Student", "Admin"
│   └── Subject.cs        ← Thay magic strings "Toan", "TiengViet",...
├── Constants/            ← MỚI
│   └── SessionKeys.cs    ← Thay magic strings "UserId", "UserRole",...
├── Middleware/            ← MỚI
│   └── AuthenticationMiddleware.cs  ← Thay thế IsTeacher(), IsAdmin() lặp lại
└── Filters/              ← MỚI
    └── AuthorizeRoleAttribute.cs    ← Custom Authorization Filter
```
==> Triển khai đúng cấu trúc này giúp cho dự án dễ bảo trì và mở rộng.
---

### A2. Tách Service Layer từ Controllers

#### A2.1 `IAuthService` / `AuthService`
Trích từ `AccountController`:
- `LoginAsync(username, password)` → Trả về `User?`
- `RegisterAsync(model)` → Trả về `(bool Success, string? Error)`
- `ValidateUsernameAsync(username)` → Kiểm tra trùng

**Code hiện tại** (AccountController.cs, dòng 41-42):
```csharp
// ❌ BAD: Logic nằm trong Controller, so sánh password plaintext
var user = await _context.Users
    .FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);
```

**Code mục tiêu**:
```csharp
// ✅ GOOD: Controller chỉ gọi Service
var user = await _authService.LoginAsync(model.Username, model.Password);
```

#### A2.2 `ILessonService` / `LessonService`
Trích từ `TeacherController`:
- `GetLessonsByTeacherAsync(teacherId)`
- `CreateLessonAsync(model, teacherId)`
- `UpdateLessonAsync(id, model, teacherId)`
- `DeleteLessonAsync(id, teacherId)`
- `GetLessonDetailAsync(lessonId, teacherId)`

#### A2.3 `IQuestionService` / `QuestionService`
Trích từ `TeacherController`:
- `CreateQuestionAsync(model, teacherId)`
- `UpdateQuestionAsync(model, teacherId)`
- `DeleteQuestionAsync(id, teacherId)`
- `ImportQuestionsFromExcelAsync(lessonId, file, teacherId)`

#### A2.4 `IClassService` / `ClassService`
Trích từ `ClassController`:
- `GetClassesAsync(grade?, page)`
- `CreateClassAsync(classRoom)`
- `UpdateClassAsync(id, classRoom)`
- `GetClassDetailsAsync(id)`
- `AssignTeacherAsync(classRoomId, teacherId, subject)`
- `RemoveAssignmentAsync(assignmentId)`

#### A2.5 `IStudentService` / `StudentService`
Trích từ `ClassController`:
- `AddStudentToClassAsync(model)`
- `RemoveStudentFromClassAsync(classRoomId, studentId)`
- `AssignExistingStudentAsync(model)`
- `ImportStudentsFromExcelAsync(model)`
- `TransferStudentAsync(model)`

#### A2.6 `ITestService` / `TestService`
Trích từ `StudentController`:
- `SubmitQuizAsync(lessonId, answers, userId)`
- `GetResultAsync(testResultId, userId)`
- `GetReviewAsync(testResultId, userId)`
- `GetHistoryAsync(userId)`

---

### A3. Tạo Enums thay Magic Strings

**File**: `Enums/UserRole.cs`
```csharp
public enum UserRole
{
    Student,
    Teacher,
    Admin
}
```

**Thay thế ở tất cả nơi dùng** magic string `"Teacher"`, `"Student"`, `"Admin"`:
- `AccountController.cs` dòng 26, 56-61
- `TeacherController.cs` dòng 23
- `ClassController.cs` dòng 23, 29, 106, 197, 288, 350, 401
- `StudentController.cs` dòng 20

**File**: `Enums/Subject.cs`
```csharp
public enum Subject
{
    Toan,
    TiengViet,
    TiengAnh,
    TNXH,
    LichSu,
    DiaLy,
    KhoaHoc,
    DaoDuc,
    AmNhac,
    MyThuat,
    TheDuc,
    TinHoc
}
```

---

### A4. Tạo Constants cho Session Keys

**File**: `Constants/SessionKeys.cs`
```csharp
public static class SessionKeys
{
    public const string UserId = "UserId";
    public const string UserName = "UserName";
    public const string UserRole = "UserRole";
}
```

**Thay thế ở tất cả nơi dùng** magic string:
- `Session.GetInt32("UserId")` → `Session.GetInt32(SessionKeys.UserId)`
- `Session.GetString("UserRole")` → `Session.GetString(SessionKeys.UserRole)`

---

### A5. Custom Authorization Filter

Thay thế hàm `IsTeacher()`, `IsAdmin()`, `IsStudent()` lặp lại ở mỗi Controller bằng một Attribute:

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeRoleAttribute : ActionFilterAttribute
{
    private readonly string[] _roles;
    
    public AuthorizeRoleAttribute(params string[] roles) { _roles = roles; }
    
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var role = context.HttpContext.Session.GetString(SessionKeys.UserRole);
        if (role == null || !_roles.Contains(role))
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }
        base.OnActionExecuting(context);
    }
}
```

**Sử dụng**:
```csharp
[AuthorizeRole("Teacher")]
public class TeacherController : Controller { ... }

[AuthorizeRole("Admin")]
public class ClassController : Controller { ... }
```

---

### A6. Đăng ký Dependency Injection trong `Program.cs`

```csharp
// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITestService, TestService>();
```

---

## 📍 Các File Bị Ảnh Hưởng

| File | Hành động | Lý do |
|------|-----------|-------|
| `Program.cs` | MODIFY | Thêm DI registrations |
| `Controllers/AccountController.cs` | MODIFY | Inject IAuthService, làm mỏng |
| `Controllers/TeacherController.cs` | MODIFY | Inject ILessonService, IQuestionService, làm mỏng |
| `Controllers/ClassController.cs` | MODIFY | Inject IClassService, IStudentService, làm mỏng |
| `Controllers/StudentController.cs` | MODIFY | Inject ITestService, làm mỏng |
| `Services/*` | NEW | Tất cả service files |
| `Enums/*` | NEW | UserRole, Subject |
| `Constants/*` | NEW | SessionKeys |
| `Filters/*` | NEW | AuthorizeRoleAttribute |

---

## ✅ Tiêu Chí Hoàn Thành

- [ ] Tất cả Controllers chỉ chứa code điều hướng (< 50 dòng mỗi action)
- [ ] Business logic nằm 100% trong Services
- [ ] Không còn magic strings cho roles, session keys
- [ ] DI đã đăng ký đầy đủ
- [ ] App build thành công, tất cả tính năng hoạt động như cũ
- [ ] Có thể viết Unit Test cho Service Layer
