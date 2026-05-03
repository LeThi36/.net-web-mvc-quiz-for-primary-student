# 📋 Kế hoạch Phát triển: Module Báo cáo kết quả dành cho Phụ huynh

> **Trạng thái**: Đề xuất sơ bộ (Chờ phản hồi)
> **Mục tiêu**: Tạo ra một trang báo cáo đẹp, gọn gàng, tập trung vào sự tiến bộ của từng học sinh để giáo viên có thể in hoặc gửi cho phụ huynh.

---

## 1. Phân biệt hai loại hình Phân tích

| Tính năng | Phân tích Giáo viên (Hiện tại) | Báo cáo Phụ huynh (Sẽ làm) |
| :--- | :--- | :--- |
| **Đối tượng** | Giáo viên xem | Phụ huynh xem |
| **Trọng tâm** | Thống kê cả lớp, độ khó câu hỏi | Kết quả cá nhân, sự tiến bộ |
| **Mục đích** | Điều chỉnh giáo án, giảng lại bài | Theo dõi sức học của con tại nhà |
| **Định dạng** | Dashboard tương tác | Trang in (A4) / Ảnh Infographic |

---

## 2. Các thông số dữ liệu trong Báo cáo Phụ huynh

Báo cáo sẽ được thiết kế theo dạng **"Phiếu kết quả học tập"** bao gồm:

### A. Thông tin cơ bản
- Họ tên học sinh, Lớp, Giáo viên chủ nhiệm.
- Tên bài học / Giai đoạn báo cáo (Tuần/Tháng).

### B. Chỉ số kết quả (Phần ngọn)
- **Điểm số**: Điểm cao nhất đạt được trong bài.
- **Xếp loại**: (Hoàn thành Xuất sắc / Tốt / Hoàn thành / Cần cố gắng).
- **Thời gian**: Tổng thời gian con đã dành để nghiên cứu và làm bài.

### C. Chỉ số quá trình (Phần gốc - Quan trọng)
- **Biểu đồ nỗ lực**: Thể hiện điểm số qua các lần làm lại (Ví dụ: Lần 1: 5đ, Lần 2: 7đ, Lần 3: 10đ -> Thể hiện sự kiên trì).
- **Thế mạnh & Lỗ hổng**: Liệt kê các chủ điểm con đã làm đúng hết và các phần con hay bị nhầm lẫn.
- **So sánh tương quan**: Hiển thị điểm của con bên cạnh **Điểm trung bình của cả lớp** (thay vì xếp hạng) để phụ huynh có cái nhìn khách quan.
- **Nhận xét của giáo viên**: Ô trống để giáo viên nhập lời khuyên nhanh (VD: *"Con cần chú ý hơn về lỗi chính tả"*).

---

## 3. Lộ trình triển khai kỹ thuật

### Giai đoạn 1: Xây dựng dữ liệu & Giao diện (Hiện tại)
1.  **UX Flow**: Thêm nút "Báo cáo" vào danh sách học sinh trong trang Analytics/Manage.
2.  **Backend**: 
    - Truy xuất lịch sử làm bài của 1 học sinh cụ thể (để vẽ biểu đồ nỗ lực cá nhân).
    - Tính toán điểm trung bình lớp để làm mốc so sánh.
3.  **UI/UX**: Thiết kế View `StudentReport.cshtml` phong cách hiện đại, tối ưu cho việc in ấn (Print-friendly).
4.  **Chức năng In**: Tích hợp nút "Xuất báo cáo" (sử dụng Window.print() với CSS @media print).

### Giai đoạn 2: Hình ảnh hóa (Sắp tới)
1.  Chuyển đổi trang HTML báo cáo thành file ảnh (JPG/PNG) để phụ huynh xem trên điện thoại không bị vỡ định dạng.

### Giai đoạn 3: Tích hợp mạng xã hội (Tương lai)
1.  Tích hợp API/Webshare để gửi trực tiếp ảnh báo cáo qua Zalo.

---

## 4. Giao diện dự kiến (Layout)
- **Header**: Logo trường + Tiêu đề "PHIẾU KẾT QUẢ HỌC TẬP".
- **Body - Left**: Biểu đồ hình nhện hoặc cột thể hiện các kỹ năng.
- **Body - Right**: Bảng điểm chi tiết và các mốc thời gian.
- **Footer**: Chữ ký giáo viên và lời nhắn nhủ.
