# 🔄 Kế Hoạch Refactoring Tổng Thể

> **Dự án**: HistoryGeoQuiz_PrimarySchool - Hệ thống Quiz cho Học sinh Tiểu học
> **Ngày tạo**: 2026-04-28
> **Trạng thái**: 📋 Planning

---

## 📊 Tổng Quan Dự Án Hiện Tại

| Thông tin | Chi tiết |
|-----------|----------|
| **Framework** | ASP.NET Core 8.0 MVC |
| **Database** | PostgreSQL (Supabase) |
| **ORM** | Entity Framework Core 8.0 |
| **Pattern** | Monolithic MVC (No Service Layer) |
| **Authentication** | Session-based (Custom, No Identity) |
| **Tổng Controllers** | 5 (Account, Class, Home, Student, Teacher) |
| **Tổng Models** | 8 (User, Lesson, Question, Answer, TestResult, TestDetail, ClassRoom, TeacherAssignment) |

---

## 🏗️ Cấu Trúc Refactoring - 5 Giai Đoạn

| # | Giai đoạn | File MD | Mức độ ưu tiên | Ước lượng |
|---|-----------|---------|-----------------|-----------|
| 1 | **Architecture** - Tái cấu trúc kiến trúc | [01-ARCHITECTURE.md](./01-ARCHITECTURE.md) | 🔴 Critical | Lớn |
| 2 | **Clean Code** - Làm sạch & chuẩn hóa code | [02-CLEAN-CODE.md](./02-CLEAN-CODE.md) | 🟠 High | Trung bình |
| 3 | **Performance** - Tối ưu hiệu suất | [03-PERFORMANCE.md](./03-PERFORMANCE.md) | 🟡 Medium | Trung bình |
| 4 | **Logic** - Sửa lỗi logic nghiệp vụ | [04-LOGIC.md](./04-LOGIC.md) | 🟠 High | Nhỏ-Trung bình |
| 5 | **Security** - Vá lỗ hổng bảo mật | [05-SECURITY.md](./05-SECURITY.md) | 🔴 Critical | Trung bình |

---

## 📁 Cấu Trúc Thư Mục

```
docs/
└── refactoring/
    ├── 00-OVERVIEW.md          ← Bạn đang ở đây
    ├── 01-ARCHITECTURE.md      ← Giai đoạn 1: Kiến trúc
    ├── 02-CLEAN-CODE.md        ← Giai đoạn 2: Clean Code
    ├── 03-PERFORMANCE.md       ← Giai đoạn 3: Performance
    ├── 04-LOGIC.md             ← Giai đoạn 4: Logic
    └── 05-SECURITY.md          ← Giai đoạn 5: Security
```

---

## 🎯 Mục Tiêu Tổng Thể

1. **Maintainability**: Code dễ bảo trì, mở rộng cho các developer mới
2. **Testability**: Có thể viết Unit Test cho business logic
3. **Security**: Loại bỏ toàn bộ lỗ hổng bảo mật nghiêm trọng
4. **Performance**: Tối ưu query, giảm tải database
5. **Scalability**: Sẵn sàng scale khi số lượng user tăng

---

## ⚠️ Nguyên Tắc Refactoring

> [!IMPORTANT]
> - Refactor từng bước nhỏ, test sau mỗi bước
> - Không thay đổi behavior hiện tại (trừ khi fix bug)
> - Backup database trước khi thay đổi schema
> - Mỗi giai đoạn có thể thực hiện độc lập

---

## 📌 Trình Tự Thực Hiện Khuyến Nghị

```mermaid
graph LR
    A[05-SECURITY] --> B[01-ARCHITECTURE]
    B --> C[02-CLEAN-CODE]
    C --> D[03-PERFORMANCE]
    D --> E[04-LOGIC]
    
    style A fill:#ff4444,color:#fff
    style B fill:#ff8800,color:#fff
    style C fill:#ffbb00,color:#000
    style D fill:#44bb44,color:#fff
    style E fill:#4488ff,color:#fff
```

> **Lý do**: Security phải fix trước vì có lỗ hổng nghiêm trọng (plaintext password, SQL injection potential). Architecture tiếp theo vì nó tạo nền tảng cho Clean Code và Performance.
