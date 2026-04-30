# Tính Năng: Theo Dõi Thời Gian Làm Bài (Time Tracking)

## 1. Mục tiêu
Theo dõi và ghi nhận chính xác thời gian (tính bằng giây) mà học sinh bỏ ra để hoàn thành một bài Quiz (Từ lúc bấm bắt đầu đến lúc nộp bài). Việc này giúp:
- Giáo viên đánh giá được tốc độ làm bài của học sinh.
- Xếp hạng học sinh nếu có số điểm bằng nhau.
- Ngăn chặn việc sử dụng tool tự động nộp bài quá nhanh.

## 2. Vấn đề hiện tại
Trong version hiện tại, thuộc tính `TimeTakenSeconds` trong bảng `TestResult` luôn bị gán cứng bằng `0` ở hàm `TestService.SubmitQuizAsync`.

## 3. Đề xuất luồng xử lý (Implementation Plan)

### Bước 1: Sửa UI (TakeQuiz.cshtml)
Ghi nhận lại thời điểm bắt đầu làm bài khi trang web được tải.
```html
<form asp-action="SubmitQuiz" method="post">
    <!-- Thêm hidden field để lưu thời gian bắt đầu (theo chuẩn UTC) -->
    <input type="hidden" name="StartTime" value="@DateTime.UtcNow.ToString("O")" />
    <!-- Các câu hỏi... -->
</form>
```

### Bước 2: Sửa Controller (StudentController.cs)
Bổ sung tham số `startTime` vào action `SubmitQuiz`.
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> SubmitQuiz(Guid lessonId, Dictionary<string, Guid> answers, DateTime startTime)
{
    // Tính toán thời gian làm bài (tính bằng giây)
    var timeTakenSeconds = (int)(DateTime.UtcNow - startTime).TotalSeconds;
    
    // Nếu timeTakenSeconds < 0 (do sai lệch đồng hồ), set về 0
    if (timeTakenSeconds < 0) timeTakenSeconds = 0;

    var testResult = await _testService.SubmitQuizAsync(lessonId, answers, GetCurrentUserId(), timeTakenSeconds);
    return RedirectToAction("Result", new { testResultId = testResult.Id });
}
```

### Bước 3: Sửa Service (TestService.cs)
Nhận tham số `timeTakenSeconds` và lưu vào DB.
```csharp
// Trong ITestService và TestService
public async Task<TestResult> SubmitQuizAsync(Guid lessonId, Dictionary<string, Guid> answers, Guid userId, int timeTakenSeconds = 0)
{
    // ... logic chấm điểm ...
    var testResult = new TestResult
    {
        // ... các field khác ...
        TimeTakenSeconds = timeTakenSeconds
    };
    // ...
}
```

## 4. Tương lai (Bảo mật nâng cao)
Sử dụng field hidden HTML dễ bị học sinh gian lận sửa thông qua F12 (Inspect Element).
Để an toàn tuyệt đối 100%:
1. Lưu `StartTime` vào `Session` lúc gọi `TakeQuiz` GET.
2. Lúc `SubmitQuiz` POST, lấy `StartTime` từ `Session` ra tính khoảng cách.
3. Nếu muốn xịn hơn nữa, lưu thẳng `QuizAttempt` vào Database chứa trạng thái (In Progress) và thời gian bắt đầu.
