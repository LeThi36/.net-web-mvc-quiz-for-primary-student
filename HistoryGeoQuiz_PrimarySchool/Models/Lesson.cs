using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HistoryGeoQuiz_PrimarySchool.Models
{
    public class Lesson : BaseEntity
    {
        [Required(ErrorMessage = "Vui lòng nhập tên bài học")]
        [Display(Name = "Tên bài học")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn môn học")]
        [Display(Name = "Môn học")]
        public string Subject { get; set; }

        [Display(Name = "Số thứ tự bài")]
        public int LessonNumber { get; set; } = 1;

        public Guid CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual User? CreatedByUser { get; set; }

        public bool IsActive { get; set; } = true;

        [Display(Name = "Lớp học")]
        public Guid? ClassRoomId { get; set; }

        [ForeignKey("ClassRoomId")]
        public virtual ClassRoom? ClassRoom { get; set; }

        // Grading Configuration
        [Display(Name = "Số câu đúng để đạt Xuất Sắc")]
        public int? QuestionCountForExcellent { get; set; }

        [Display(Name = "Số câu đúng để đạt Tốt")]
        public int? QuestionCountForGood { get; set; }

        [Display(Name = "Thời gian làm bài (phút)")]
        [Range(1, 180, ErrorMessage = "Thời gian phải từ 1-180 phút")]
        public int? TimeLimitMinutes { get; set; }

        // Navigation properties
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
        public virtual ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
    }
}
