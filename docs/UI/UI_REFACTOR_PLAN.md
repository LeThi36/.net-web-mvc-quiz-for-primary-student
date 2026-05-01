# Kế Hoạch Refactor UI Views Theo DESIGN.md

## Summary
Refactor toàn bộ UI trong `HistoryGeoQuiz_PrimarySchool/Views` theo design system "Sunny Blue Education".

Mục tiêu là đổi giao diện từ palette cam/teal hiện tại sang Sunny Blue, chuẩn hóa typography Plus Jakarta Sans/Lexend, giảm inline style, giữ nguyên logic Razor, model binding, route, form action và JavaScript hành vi hiện có.

## Key Changes
- Cập nhật `_Layout.cshtml`: đổi font Google sang Plus Jakarta Sans + Lexend, sửa brand/nav/footer theo tông Sunny Blue, giữ nguyên session/role navigation và logout POST.
- Refactor `wwwroot/css/site.css`: thay CSS variables bằng token từ `DESIGN.md`, thêm component class dùng chung cho page header, cards, buttons, badges, form controls, tables, quiz answers, empty states, analytics blocks.
- Refactor nhóm Student views: dashboard và quiz cần thoáng, gamified, card lớn, progress/timer rõ, answer option dễ bấm, result/review nổi bật đúng/sai.
- Refactor nhóm Teacher/Admin views: dashboard, class/user management, import, analytics dùng layout gọn hơn, table dễ scan, card có outline nhẹ, chỉ dùng shadow cho vùng quan trọng.
- Refactor Auth/Profile/Home/Error views: thống nhất header, form, CTA, trạng thái lỗi/trống; bỏ inline style không cần thiết.
- Không thay đổi `Models`, `Controllers`, ViewModels, route names, form field names, validation attributes hoặc nghiệp vụ submit quiz/import/delete.

## Implementation Plan
- Tạo lớp nền design system trong CSS trước: tokens, typography, button depth effect, cards, dashboard shell, table, form, alert, badge, empty state, responsive rules.
- Chuẩn hóa Razor markup theo component class mới, ưu tiên thay `style="..."`, `bg-primary`, `text-primary`, `shadow-sm`, `rounded-4` rải rác bằng class semantic như `app-page-header`, `app-card`, `teacher-panel`, `student-card`, `quiz-option`.
- Giữ Bootstrap 5 và Bootstrap Icons vì repo đang phụ thuộc sẵn; không thêm framework UI mới.
- Với JavaScript inline hiện có, chỉ đổi selector/class nếu cần cho UI mới; không thay đổi logic countdown, confirm submit, print report hoặc preview avatar.
- Kiểm tra lại tiếng Việt hiển thị trong layout/views sau refactor để tránh lỗi encoding/mojibake ở title, nav, button, alert và empty state.

## Test Plan
- Chạy `dotnet build` cho project ASP.NET Core.
- Kiểm tra thủ công các luồng chính: Home, Login/Register, Student Dashboard, Take Quiz, Review/Result, Teacher Dashboard, Manage Questions, Analytics, Class/User management, Import pages, Profile.
- Kiểm tra responsive ở desktop, tablet và mobile: nav collapse, bảng không vỡ layout, quiz answer không tràn chữ, form controls dễ bấm.
- Kiểm tra accessibility cơ bản: contrast text trên nền màu, focus state input/button, nút icon vẫn có text hoặc title khi cần.
- Kiểm tra nghiệp vụ không đổi: submit form, validation, anti-forgery logout/delete, timer auto-submit, print analytics.

## Assumptions
- File kế hoạch được lưu tại `docs/UI/UI_REFACTOR_PLAN.md`.
- Phạm vi refactor là UI trong `Views` và CSS hỗ trợ trong `wwwroot/css/site.css`; không refactor backend.
- Bootstrap và Bootstrap Icons tiếp tục được dùng.
- DESIGN.md là nguồn chuẩn duy nhất cho màu, typography, spacing, shape và phong cách visual.
