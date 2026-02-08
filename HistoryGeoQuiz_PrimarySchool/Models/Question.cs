using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HistoryGeoQuiz_PrimarySchool.Models
{
    public class Question
    {
        public int Id { get; set; }

        public int LessonId { get; set; }

        [ForeignKey("LessonId")]
        public virtual Lesson? Lesson { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung câu hỏi")]
        [Display(Name = "Nội dung câu hỏi")]
        public string QuestionText { get; set; } = string.Empty;

        [Display(Name = "Hình ảnh minh họa")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Thứ tự câu hỏi")]
        public int OrderIndex { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
        public virtual ICollection<TestDetail> TestDetails { get; set; } = new List<TestDetail>();
    }
}
