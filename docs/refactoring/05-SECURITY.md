# 🛡️ Giai Đoạn 5: Security - Vá Lỗ Hổng Bảo Mật

> **Mức độ ưu tiên**: 🔴 Critical — **NÊN LÀM ĐẦU TIÊN**
> **Ước lượng**: Trung bình (2-3 ngày)
> **Phụ thuộc**: Không

---

> [!CAUTION]
> Dự án hiện có **nhiều lỗ hổng bảo mật nghiêm trọng** cần được vá NGAY LẬP TỨC trước khi triển khai production. Một số lỗ hổng cho phép chiếm toàn bộ quyền hệ thống.

---

## 📝 Danh Sách Lỗ Hổng

### S1. 🔴 CRITICAL: Mật khẩu lưu dạng Plaintext

**Mức độ**: OWASP A02:2021 – Cryptographic Failures

**Vị trí**:
| File | Dòng | Code |
|------|------|------|
| AccountController.cs | 42 | `u.Password == model.Password` |
| AccountController.cs | 91 | `Password = model.Password, // In production, hash this!` |
| ClassController.cs | 301 | `Password = model.Password, // In production, hash this!` |
| ClassController.cs | 521 | `Password = password, // In production, hash this!` |

**Mô tả**: Mật khẩu được lưu trực tiếp vào database dưới dạng plaintext. Nếu database bị leak → **TOÀN BỘ mật khẩu user bị lộ**.

**Giải pháp**: Sử dụng BCrypt để hash mật khẩu

```bash
# Cài đặt package
dotnet add package BCrypt.Net-Next
```

```csharp
// ✅ Hash khi lưu
using BCrypt.Net;

var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
var user = new User { Password = hashedPassword };

// ✅ Verify khi login
var user = await _context.Users
    .FirstOrDefaultAsync(u => u.Username == model.Username);

if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
{
    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng!");
    return View(model);
}
```

> [!IMPORTANT]
> Sau khi triển khai hash, cần migration script để hash lại tất cả mật khẩu hiện tại trong database.
> ```sql
> -- Migration Script: Hash existing passwords (run once)
> -- Phải dùng code C#, không thể hash BCrypt bằng SQL thuần
> ```

---

### S2. 🔴 CRITICAL: Database Credentials trong Source Code

**Mức độ**: OWASP A07:2021 – Identification and Authentication Failures

**Vị trí**: `appsettings.Development.json` dòng 9:
```json
"DefaultConnection": "Host=aws-1-ap-south-1.pooler.supabase.com;Port=5432;
Database=postgres;Username=postgres.zivyahyzmizvffwnfbsb;
Password=volethi2003!;SSL Mode=Require;Trust Server Certificate=true"
```

**Mô tả**: 
- Connection string chứa **password thật** được commit vào Git ==> xem đã rm khỏi track của git chưa là được, còn tôi đã đưa file này vào .gitignore rồi
- Bất kỳ ai truy cập repository đều thấy database credentials
- Cho phép truy cập trực tiếp vào Supabase database

**Giải pháp**:

1. **Ngay lập tức**: Đổi password database trên Supabase
2. **Sử dụng User Secrets** (development):
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Password=NEW_PASSWORD;..."
```

3. **Sử dụng Environment Variables** (production):
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": ""  // Empty - sẽ được override bởi env var
  }
}
```

4. **Thêm vào `.gitignore`**:
```
appsettings.Development.json
```

5. **Tạo `appsettings.Development.json.example`** (mẫu không chứa password)

---

### S3. 🔴 CRITICAL: Developer Exception Page trong Production

**Mức độ**: OWASP A05:2021 – Security Misconfiguration

**Vị trí**: `Program.cs` dòng 28-33:
```csharp
// TEMP: Always show detailed errors for debugging
app.UseDeveloperExceptionPage();  // ← LUÔN bật, kể cả production!
```

**Mô tả**: Developer Exception Page hiển thị:
- Stack trace đầy đủ
- Source code snippets
- Environment variables (có thể chứa connection string)
- Routing details

**Giải pháp**:
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
```

---

### S4. 🔴 CRITICAL: ValidateAntiForgeryToken bị tắt

**Mức độ**: OWASP A01:2021 – Broken Access Control (CSRF)

**Vị trí**: `TeacherController.cs` dòng 98:
```csharp
// [ValidateAntiForgeryToken] // TODO: Temporarily disabled for debugging
```

**Mô tả**: Tắt CSRF protection cho endpoint `CreateLesson POST`. Attacker có thể tạo form ẩn trên website khác, lừa giáo viên click → tự động tạo lesson.

**Giải pháp**: Bật lại `[ValidateAntiForgeryToken]`:
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CreateLesson(CreateLessonViewModel model)
```

---

### S5. 🔴 CRITICAL: Người dùng tự đăng ký Role Admin

**Mức độ**: OWASP A01:2021 – Broken Access Control (Privilege Escalation)

**Vị trí**: `AccountController.cs` dòng 88-95:
```csharp
var user = new User
{
    Role = model.Role, // ← Giá trị từ form, có thể là "Admin"!
};
```

**Mô tả**: Form đăng ký cho phép user chọn role. Attacker có thể sửa HTML hoặc gửi POST request trực tiếp với `Role=Admin` → chiếm quyền Admin.

**Giải pháp**:
```csharp
// ✅ FIX: Whitelist roles
var allowedRoles = new[] { "Student", "Teacher" };
if (!allowedRoles.Contains(model.Role))
{
    ModelState.AddModelError("Role", "Vai trò không hợp lệ");
    return View(model);
}
```

Hoặc tốt hơn: **Không cho phép đăng ký Teacher**, chỉ Admin mới có thể tạo tài khoản Teacher.

---

### S6. 🟠 HIGH: Thiếu Authorization Check trên nhiều Endpoints

**Mức độ**: OWASP A01:2021 – Broken Access Control

| Endpoint | File | Dòng | Vấn đề |
|----------|------|------|--------|
| `POST AssignTeacher` | ClassController.cs | 210 | Không check IsAdmin |
| `POST RemoveAssignment` | ClassController.cs | 234 | Không check IsAdmin |
| `GET DownloadStudentTemplate` | ClassController.cs | 548 | Không check auth |
| `GET DownloadTemplate` | TeacherController.cs | 484 | Chỉ check IsTeacher |

**Giải pháp**: Thêm authorization check cho mỗi endpoint (xem chi tiết ở 04-LOGIC.md L4, L5)

---

### S7. 🟠 HIGH: Session Fixation & Security Headers

**Mức độ**: OWASP A07:2021 – Identification and Authentication Failures

**Vị trí**: `Program.cs`

**Các vấn đề**:

#### S7.1 Thiếu Session Regeneration sau Login
```csharp
// ❌ Không regenerate session ID sau login
HttpContext.Session.SetInt32("UserId", user.Id);
// Attacker có thể fix session ID trước khi user login
```

**Giải pháp**:
```csharp
// ✅ Clear session cũ, tạo session mới
HttpContext.Session.Clear();
HttpContext.Session.SetInt32("UserId", user.Id);
```

#### S7.2 Thiếu Security Headers
```csharp
// ✅ Thêm vào Program.cs
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});
```

#### S7.3 Session Cookie thiếu Secure flag
```csharp
// ❌ Hiện tại
options.Cookie.HttpOnly = true;
options.Cookie.IsEssential = true;
// ← Thiếu: options.Cookie.SecurePolicy

// ✅ Thêm
options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
options.Cookie.SameSite = SameSiteMode.Strict;
```

---

### S8. 🟠 HIGH: Thiếu Input Validation & Sanitization

**Mức độ**: OWASP A03:2021 – Injection

#### S8.1 Excel Upload không kiểm tra file size
```csharp
// ❌ Không giới hạn file size
if (file == null || file.Length == 0) ...
// → Attacker có thể upload file 100MB+, gây DoS
```

**Giải pháp**:
```csharp
const long MaxFileSize = 5 * 1024 * 1024; // 5MB
if (file.Length > MaxFileSize)
{
    ModelState.AddModelError("", "File không được vượt quá 5MB");
    return View();
}
```

#### S8.2 Excel data không được sanitize
```csharp
// ❌ Dữ liệu từ Excel đưa thẳng vào DB
string qText = GetCell("NoiDungCauHoi", "QuestionText");
var question = new Question { QuestionText = qText };
// → Có thể chứa XSS payload: <script>alert('hacked')</script>
```

**Giải pháp**: Sanitize HTML trong input
```csharp
using System.Web;
string sanitized = HttpUtility.HtmlEncode(qText);
```

#### S8.3 Username không validate đặc biệt ký tự khi tạo inline
```csharp
// ClassController AddStudent có RegularExpression validation
// Nhưng ImportStudents từ Excel KHÔNG có!
```

---

### S9. 🟡 MEDIUM: Thiếu Rate Limiting

**Mức độ**: OWASP A04:2021 – Insecure Design

**Vị trí**: `AccountController.cs` Login endpoint

**Mô tả**: Không có giới hạn số lần đăng nhập sai → Brute force attack

**Giải pháp** (ASP.NET Core 7+):
```csharp
// Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(15);
    });
});

// AccountController.cs
[EnableRateLimiting("login")]
[HttpPost]
public async Task<IActionResult> Login(LoginViewModel model) ...
```

---

### S10. 🟡 MEDIUM: Thiếu HTTPS Enforcement

**Vị trí**: `Program.cs`

```csharp
// ❌ Thiếu HTTPS redirect
// app.UseHttpsRedirection();  ← Không có!
```

**Giải pháp**:
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}
```

---

### S11. 🟡 MEDIUM: Error Messages lộ thông tin nhạy cảm

**TeacherController.cs** dòng 167:
```csharp
// ❌ Lộ chi tiết exception cho user
TempData["ErrorMessage"] = $"Lỗi khi lưu bài học: {ex.Message}";
// ex.Message có thể chứa: "Connection string", "Table name", "Column info"
```

**Giải pháp**:
```csharp
// ✅ Generic error message cho user, log chi tiết
_logger.LogError(ex, "Error saving lesson");
TempData["ErrorMessage"] = "Có lỗi xảy ra. Vui lòng thử lại sau.";
```

---

### S12. 🟡 MEDIUM: Logout không dùng POST

**AccountController.cs** dòng 104-108:
```csharp
// ❌ GET request cho logout → CSRF: Attacker có thể embed <img src="/Account/Logout"> 
public IActionResult Logout()
{
    HttpContext.Session.Clear();
    return RedirectToAction("Login");
}
```

**Giải pháp**:
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Logout()
{
    HttpContext.Session.Clear();
    return RedirectToAction("Login");
}
```

---

## 📊 Tổng Hợp Lỗ Hổng (Theo OWASP Top 10)

| OWASP Category | Lỗ hổng | Mức độ | Task |
|----------------|---------|--------|------|
| A01: Broken Access Control | Role escalation qua Register | 🔴 Critical | S5 |
| A01: Broken Access Control | Missing auth checks | 🟠 High | S6 |
| A01: Broken Access Control | CSRF - AntiForgeryToken tắt | 🔴 Critical | S4 |
| A01: Broken Access Control | Logout via GET | 🟡 Medium | S12 |
| A02: Cryptographic Failures | Plaintext passwords | 🔴 Critical | S1 |
| A03: Injection | Unsanitized Excel data | 🟠 High | S8 |
| A04: Insecure Design | No rate limiting | 🟡 Medium | S9 |
| A05: Security Misconfiguration | DevExceptionPage in prod | 🔴 Critical | S3 |
| A05: Security Misconfiguration | DB creds in source | 🔴 Critical | S2 |
| A05: Security Misconfiguration | No HTTPS | 🟡 Medium | S10 |
| A07: Auth Failures | No session regen | 🟠 High | S7 |
| A09: Logging Failures | Error msg leaks info | 🟡 Medium | S11 |

---

## 🚨 Thứ Tự Fix Khuyến Nghị

```
1. S2 → Đổi password database NGAY (credentials đã leak trong Git)
2. S1 → Hash passwords (BCrypt)  
3. S5 → Block Admin registration
4. S4 → Bật lại ValidateAntiForgeryToken
5. S3 → Fix Developer Exception Page
6. S6 → Thêm auth checks
7. S7 → Session security
8. S8 → Input validation
9. S9-S12 → Các fix còn lại
```

---

## ✅ Tiêu Chí Hoàn Thành

- [ ] Tất cả mật khẩu được hash bằng BCrypt
- [ ] Không còn credentials trong source code
- [ ] Developer Exception Page chỉ hiện trong Development
- [ ] `[ValidateAntiForgeryToken]` bật cho tất cả POST endpoints
- [ ] Đăng ký không cho phép role Admin
- [ ] Tất cả endpoints có authorization check
- [ ] Security headers được thêm
- [ ] Session cookie có Secure flag
- [ ] File upload có giới hạn size
- [ ] Input data được sanitize
- [ ] Error messages không lộ thông tin nhạy cảm
- [ ] Logout dùng POST method
