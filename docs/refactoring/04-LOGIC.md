# 🧠 Giai Đoạn 4: Logic - Sửa Lỗi Logic Nghiệp Vụ

> **Mức độ ưu tiên**: 🟠 High
> **Ước lượng**: Nhỏ-Trung bình (1-2 ngày)
> **Phụ thuộc**: Không

---

## 📋 Tóm Tắt Vấn Đề

Code hiện tại chứa một số lỗi logic nghiệp vụ có thể gây ra hành vi sai hoặc không mong muốn. Các vấn đề được phân loại theo mức độ nghiêm trọng.

---

## 📝 Danh Sách Task

### L1. 🔴 `IsStudent()` không kiểm tra đúng Role

**StudentController.cs** dòng 18-22:
```csharp
// ❌ BUG: Chỉ check userId != null, KHÔNG check role = "Student"
private bool IsStudent()
{
    var userId = HttpContext.Session.GetInt32("UserId");
    return userId != null;
    // → BẤT KỲ user đã đăng nhập (Teacher, Admin) đều pass kiểm tra này!
}
```

**Hậu quả**:
- Teacher có thể truy cập trang Student Dashboard
- Admin có thể làm quiz, nộp bài
- Vi phạm nguyên tắc phân quyền

**Giải pháp**:
```csharp
// ✅ FIX: Check cả role
private bool IsStudent()
{
    var role = HttpContext.Session.GetString("UserRole");
    return role == "Student";
}
```

---

### L2. 🔴 `GetCurrentUserId()` trả về 0 khi chưa đăng nhập

**TeacherController.cs** & **StudentController.cs**:
```csharp
// ❌ BUG: Trả về 0 thay vì handle null
private int GetCurrentUserId()
{
    return HttpContext.Session.GetInt32("UserId") ?? 0;
}
```

**Hậu quả**:
- Nếu session hết hạn, userId = 0
- Query `WHERE CreatedByUserId == 0` vẫn thực hiện → Có thể trả về dữ liệu ngoài ý muốn nếu có user có Id=0
- Các action dùng `GetCurrentUserId()` mà không check `IsTeacher()` trước sẽ hoạt động sai

**Giải pháp**:
```csharp
// ✅ FIX: Trả về nullable, force check
private int? GetCurrentUserId()
{
    return HttpContext.Session.GetInt32("UserId");
}

// Hoặc throw exception nếu không có user
private int GetRequiredUserId()
{
    return HttpContext.Session.GetInt32("UserId")
        ?? throw new UnauthorizedAccessException("User not authenticated");
}
```

---

### L3. 🟠 Register cho phép chọn Role "Admin"

**AccountController.cs** dòng 88-95 & **AccountViewModels.cs** dòng 43:
```csharp
// ❌ BUG: User tự đăng ký có thể chọn Role = "Admin"
var user = new User
{
    Role = model.Role, // ← Giá trị từ form, user có thể gửi "Admin"!
};
```

**RegisterViewModel.cs**:
```csharp
[Required(ErrorMessage = "Vui lòng chọn vai trò")]
public string Role { get; set; } = "Student";
// ← Không có validation giới hạn giá trị!
```

**Hậu quả**:
- Bất kỳ ai cũng có thể tạo tài khoản Admin
- Toàn bộ hệ thống bị chiếm quyền

**Giải pháp**:
```csharp
// ✅ FIX 1: Validate role trên server
if (model.Role != "Student" && model.Role != "Teacher")
{
    ModelState.AddModelError("Role", "Vai trò không hợp lệ");
    return View(model);
}

// ✅ FIX 2 (Tốt hơn): Chỉ cho phép đăng ký Student, Teacher phải do Admin tạo
var user = new User
{
    Role = "Student", // Luôn là Student khi tự đăng ký
};
```

---

### L4. 🟠 `AssignTeacher` thiếu kiểm tra quyền

**ClassController.cs** dòng 208-229:
```csharp
// ❌ BUG: Không có kiểm tra IsAdmin()
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> AssignTeacher(int classRoomId, int teacherId, string subject)
{
    // ← Thiếu: if (!IsAdmin()) return RedirectToAction("Login", "Account");
    
    var exists = await _context.TeacherAssignments
        .AnyAsync(ta => ta.ClassRoomId == classRoomId && ...);
    // ...
}
```

**Hậu quả**: Bất kỳ user nào gửi POST request đều có thể phân công giáo viên

**Giải pháp**: Thêm kiểm tra `IsAdmin()` hoặc dùng `[AuthorizeRole("Admin")]`

---

### L5. 🟠 `RemoveAssignment` thiếu kiểm tra quyền

**ClassController.cs** dòng 232-245:
```csharp
// ❌ BUG: Không có kiểm tra quyền
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> RemoveAssignment(int assignmentId)
{
    // ← Thiếu kiểm tra IsAdmin()
    var assignment = await _context.TeacherAssignments.FindAsync(assignmentId);
    // ...
}
```

**Giải pháp**: Tương tự L4

---

### L6. 🟠 `TransferStudent` thiếu kiểm tra quyền target class

**ClassController.cs** dòng 628-677:
```csharp
// ❌ BUG: Chỉ check quyền cho source class, không check target class
public async Task<IActionResult> TransferStudent(TransferStudentViewModel model)
{
    var student = await _context.Users...
    // Không check: if (!await CanManageClass(model.TargetClassRoomId)) ...
    
    student.ClassRoomId = model.TargetClassRoomId; // ← Chuyển vào lớp mà mình không có quyền
}
```

**Giải pháp**: Thêm check `CanManageClass` cho cả target class, hoặc yêu cầu Admin

---

### L7. 🟡 `SubmitQuiz` - Answer key parsing dễ bị lỗi

**StudentController.cs** dòng 169-173:
```csharp
// ❌ FRAGILE: Parse string key, dễ throw exception
var answerDict = answers.ToDictionary(
    kvp => int.Parse(kvp.Key.Replace("answers[", "").Replace("]", "")),
    kvp => kvp.Value
);
```

**Hậu quả**:
- Nếu key format thay đổi → `FormatException` → 500 Error
- Không có try-catch

**Giải pháp**: Sử dụng `SubmitQuizViewModel` thay vì `Dictionary<string, int>`:
```csharp
// ✅ FIX: Dùng proper model binding
[HttpPost]
public async Task<IActionResult> SubmitQuiz(SubmitQuizViewModel model)
{
    // model.Answers đã là Dictionary<int, int> sẵn
}
```

---

### L8. 🟡 `SubmitQuiz` không check student có thuộc lớp không

**StudentController.cs** dòng 151-167:
```csharp
// ❌ BUG: Không verify student thuộc lớp có lesson này
public async Task<IActionResult> SubmitQuiz(int lessonId, Dictionary<string, int> answers)
{
    var lesson = await _context.Lessons...
    // ← Thiếu: Check lesson.ClassRoomId == student.ClassRoomId
    // (TakeQuiz có check nhưng SubmitQuiz thì KHÔNG!)
}
```

**Hậu quả**: Student có thể submit quiz cho bài học của lớp khác bằng cách gửi POST trực tiếp

**Giải pháp**: Thêm check tương tự `TakeQuiz`

---

### L9. 🟡 `EditLesson` không load ClassRooms cho dropdown

**TeacherController.cs** dòng 176-196:
```csharp
// ❌ BUG: EditLesson GET không set ViewBag.ClassRooms
public async Task<IActionResult> EditLesson(int id)
{
    // ... load lesson data
    // ← THIẾU: ViewBag.ClassRooms
    return View(model);
}
```

**Hậu quả**: Nếu view cần dropdown ClassRoom → sẽ hiện trống hoặc crash

---

### L10. 🟡 `EditLesson` không cập nhật ClassRoomId, QuestionCountForExcellent, QuestionCountForGood

**TeacherController.cs** dòng 216-219:
```csharp
// ❌ BUG: Thiếu cập nhật một số field
lesson.Title = model.Title;
lesson.Description = model.Description;
lesson.Subject = model.Subject;
lesson.LessonNumber = model.LessonNumber;
// ← THIẾU: lesson.ClassRoomId = model.ClassRoomId;
// ← THIẾU: lesson.QuestionCountForExcellent = model.QuestionCountForExcellent;
// ← THIẾU: lesson.QuestionCountForGood = model.QuestionCountForGood;
```

---

### L11. 🟡 `DownloadTemplate` & `DownloadStudentTemplate` thiếu kiểm tra quyền đầy đủ

**ClassController.cs** dòng 548:
```csharp
// ❌ BUG: Không check IsAdmin() hoặc CanManageClass()
public IActionResult DownloadStudentTemplate()
{
    // Bất kỳ ai cũng download được template
}
```

---

### L12. 🟢 `TimeTakenSeconds` luôn là 0

**StudentController.cs** dòng 207:
```csharp
// ❌ MISSING FEATURE: Không track thời gian làm bài
TimeTakenSeconds = 0  // ← Luôn là 0
```

**Giải pháp**: Track thời gian bắt đầu quiz (TakeQuiz GET) bằng hidden field hoặc session, tính delta khi submit

---

## 📊 Tổng Hợp Lỗi Logic

| # | Mức độ | Vấn đề | File |
|---|--------|--------|------|
| L1 | 🔴 Critical | IsStudent() không check role | StudentController |
| L2 | 🔴 Critical | GetCurrentUserId() trả về 0 | Teacher/StudentController |
| L3 | 🔴 Critical | Register cho phép role Admin | AccountController |
| L4 | 🟠 High | AssignTeacher thiếu auth check | ClassController |
| L5 | 🟠 High | RemoveAssignment thiếu auth check | ClassController |
| L6 | 🟠 High | TransferStudent không check target | ClassController |
| L7 | 🟡 Medium | SubmitQuiz fragile parsing | StudentController |
| L8 | 🟡 Medium | SubmitQuiz không check lớp | StudentController |
| L9 | 🟡 Medium | EditLesson thiếu ClassRooms | TeacherController |
| L10 | 🟡 Medium | EditLesson thiếu update fields | TeacherController |
| L11 | 🟡 Medium | Download template không auth | ClassController |
| L12 | 🟢 Low | TimeTakenSeconds = 0 | StudentController |

---

## ✅ Tiêu Chí Hoàn Thành

- [ ] `IsStudent()` kiểm tra đúng role
- [ ] `GetCurrentUserId()` handle null đúng cách
- [ ] Register không cho phép tạo Admin
- [ ] Tất cả action có kiểm tra quyền phù hợp
- [ ] SubmitQuiz validate student thuộc lớp
- [ ] EditLesson cập nhật đầy đủ fields
- [ ] Tất cả download endpoint có auth check
