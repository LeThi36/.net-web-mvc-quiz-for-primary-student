# 🛡️ Kế hoạch Chuyển đổi Xác thực: Session sang JWT

> **Mục tiêu**: Thay thế cơ chế xác thực dựa trên Session truyền thống bằng JSON Web Token (JWT) để tăng tính bảo mật, hiệu năng và sẵn sàng cho các tích hợp bên thứ ba (Zalo, Mobile App).

---

## 1. Tại sao nên chuyển sang JWT?

- **Stateless (Không lưu trạng thái)**: Server không cần lưu trữ dữ liệu phiên làm việc, giúp tiết kiệm RAM và dễ dàng mở rộng.
- **Security (Bảo mật)**: Token được mã hóa và ký số, chống giả mạo tốt hơn Session ID thông thường.
- **Cross-Platform**: JWT là tiêu chuẩn chung, giúp Mobile App hoặc các dịch vụ khác có thể sử dụng chung hệ thống xác thực.

---

## 2. Lộ trình Triển khai (Phân kỳ)

### Phase 1: Cấu hình & Hạ tầng (Infrastructure)
1.  **Cài đặt thư viện**: Thêm `Microsoft.AspNetCore.Authentication.JwtBearer`.
2.  **Cấu hình Secret Key**: Thêm các thông số `JwtSettings` (Secret, Issuer, Audience, Expiry) vào `appsettings.json`.
3.  **Cập nhật Program.cs**: Đăng ký Middleware Authentication & Authorization để hỗ trợ JwtBearer.

### Phase 2: Dịch vụ Tạo Token (Service Layer)
1.  **Cập nhật IAuthService**: Thêm phương thức `GenerateJwtToken(User user)`.
2.  **Logic Logic**: Sau khi kiểm tra mật khẩu thành công, thay vì ghi Session, Service sẽ trả về chuỗi JWT Token.

### Phase 3: Cơ chế Lưu trữ & Login (Controller Layer)
1.  **Lưu trữ Token**: Để phù hợp với ứng dụng Web MVC hiện tại, tôi đề xuất lưu JWT vào **HTTP-Only Cookie**.
    - *Lợi ích*: Trình duyệt tự động gửi Token đi mỗi khi gọi request, đồng thời JavaScript không thể đọc được Token (Chống XSS).
2.  **AccountController**: Cập nhật Action `Login` để tạo Token và ghi vào Cookie.

### Phase 4: Đồng bộ hóa Giao diện & Phân quyền
1.  **BaseController**: Cập nhật `GetCurrentUserId()` và các Helper để lấy dữ liệu từ `User.Claims` thay vì `Session`.
2.  **View Layer**: Thay thế các dòng `@Context.Session.GetString(...)` bằng `@User.FindFirst(...)` hoặc các Extension methods.
3.  **Logout**: Xóa Cookie JWT khi người dùng đăng xuất.

---

## 3. Cấu trúc Token Dự kiến (Claims)

Mỗi Token sẽ chứa:
- `sub`: UserId (ID người dùng).
- `unique_name`: Username.
- `role`: Quyền (Admin/Teacher/Student).
- `given_name`: FullName (Để hiển thị trên Navbar).
- `avatar`: AvatarUrl (Để hiển thị ảnh đại diện ngay lập tức mà không cần query DB).
- `exp`: Thời gian hết hạn.

---

## 4. Những thay đổi quan trọng đối với Code hiện tại

### A. File `appsettings.json` (Thêm mới)
```json
"JwtSettings": {
    "Secret": "Chuoi_Bi_Mat_Cuc_Ky_Dai_Va_Kho_Doan_123456",
    "Issuer": "SunnyQuizSystem",
    "Audience": "SunnyQuizUsers",
    "ExpiryMinutes": 1440
}
```

### B. File `BaseController.cs` (Cập nhật)
```csharp
protected Guid GetCurrentUserId() {
    // Trước: return Guid.Parse(HttpContext.Session.GetString("UserId"));
    // Sau:
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;
}
```

---

## 5. Rủi ro & Giải pháp

- **Rủi ro**: JWT hết hạn khiến người dùng bị logout đột ngột.
- **Giải pháp**: 
    - Thiết lập thời gian hết hạn hợp lý (ví dụ 1 ngày).
    - **Refresh Token**: Đã đưa vào danh sách phát triển ở Giai đoạn sau (Chi tiết tại `docs/features/Future_Roadmap.md`).

---
*Tài liệu này được soạn thảo để thảo luận trước khi thực hiện thay đổi mã nguồn chính thức.*
