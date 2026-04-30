# Bản Đồ Phát Triển Tính Năng (EdTech Roadmap)

Tài liệu này lưu trữ các ý tưởng phát triển ứng dụng **Học Tập Lịch Sử - Địa Lý (Tiểu Học)** nhằm biến hệ thống từ một công cụ quản lý bài tập cơ bản thành một sản phẩm EdTech mang tính tương tác và hấp dẫn cao đối với học sinh Tiểu học.

---

## 🎮 Nhóm 1: Trò Chơi Hóa (Gamification)
*Tập trung vào đối tượng Học sinh. Trẻ em học tốt nhất khi chúng cảm thấy như đang chơi.*

- [ ] **Hệ thống Điểm kinh nghiệm (EXP) & Lên cấp (Leveling):** 
  - Mỗi câu trả lời đúng được `+10 EXP`. 
  - Đạt mốc EXP nhất định sẽ được thăng cấp (Ví dụ: *Tân Binh ➡️ Học Giả ➡️ Nhà Sử Học Nhí*).
- [ ] **Bộ sưu tập Huy hiệu (Badges):** 
  - Hệ thống tự động trao huy hiệu khi đạt thành tựu: *Chuỗi 7 ngày học chăm, Trả lời đúng 10 câu liên tiếp, Đạt 3 điểm Xuất sắc liên tiếp.*
- [ ] **Cửa hàng Avatar (Avatar Shop):** 
  - Sử dụng điểm EXP kiếm được để "mua" hoặc mở khóa các ảnh đại diện (avatar) hình thú cưng, nhân vật hoạt hình.
- [ ] **Bảng xếp hạng (Leaderboard):** 
  - Bảng vinh danh Top 3 hoặc Top 5 của tuần/tháng trong phạm vi Lớp học. (Lưu ý: Chỉ hiện Top trên để tránh gây áp lực cho học sinh nhóm dưới).

---

## 👩‍🏫 Nhóm 2: Công cụ Hỗ trợ Giáo Viên (Teacher Tools)
*Giúp giáo viên tiết kiệm thời gian, dễ dàng quản lý và nhìn ra lỗ hổng kiến thức của lớp.*

- [ ] **Phân tích Câu hỏi khó (Question Analytics):** 
  - Báo cáo thống kê (Ví dụ: *"Câu hỏi số 3 có 80% học sinh làm sai"*). Giúp giáo viên biết nên giảng lại phần kiến thức nào trên lớp.
- [ ] **Chế độ Đấu Trường (Kahoot-style Mode):** 
  - Giáo viên tạo một mã PIN, cả lớp nhập mã vào điện thoại/máy tính bảng để thi trắc nghiệm trực tiếp.
- [ ] **Ngân hàng Câu hỏi chung (Shared Question Bank):** 
  - Các giáo viên trong cùng trường có thể chia sẻ câu hỏi cho nhau trên hệ thống thay vì trao đổi qua file Excel.

---

## 🎨 Nhóm 3: Trải Nghiệm Người Dùng (UI/UX & Interactive)
*Nâng cao mức độ tương tác và cảm xúc khi sử dụng ứng dụng.*

- [ ] **Hiệu ứng âm thanh & hình ảnh (Sound & Confetti):** 
  - Màn hình tung pháo hoa (Confetti) kèm tiếng vỗ tay khi nộp bài đạt điểm Xuất sắc. 
  - Có âm thanh phản hồi (ting ting/bíp bíp) khi chọn đáp án đúng/sai lúc xem lại bài.
- [ ] **Hỗ trợ đọc văn bản (Text-to-Speech):** 
  - Bổ sung nút bấm (icon cái loa) để tự động đọc to nội dung câu hỏi và đáp án, hỗ trợ các học sinh lớp nhỏ (lớp 1, 2) có tốc độ đọc chữ chậm.
- [ ] **Chế độ Thử thách thời gian (Time Challenge):** 
  - (*Đã có file Spec: `TimeTracking.md`*) Đồng hồ đếm ngược trên màn hình để luyện phản xạ.

---

## 👨‍👩‍👧‍👦 Nhóm 4: Cổng Phụ Huynh (Parent Connection)
*Tăng cường sự kết nối giữa nhà trường và gia đình.*

- [ ] **Xuất & Gửi Báo Cáo Tự Động:** 
  - Giáo viên bấm 1 nút để xuất file báo cáo điểm số dưới dạng hình ảnh hoặc PDF đẹp mắt gửi qua Zalo/Email cho phụ huynh.
- [ ] **Sổ liên lạc điện tử / Tra cứu mã:** 
  - Phụ huynh nhập Mã Học Sinh (Student ID) ở một trang public để xem biểu đồ tiến trình học tập của con (Không cần tài khoản phức tạp).
