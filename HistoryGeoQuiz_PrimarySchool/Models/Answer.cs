using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HistoryGeoQuiz_PrimarySchool.Models
{
    public class Answer : BaseEntity
    {
        public Guid QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question? Question { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung đáp án")]
        [Display(Name = "Nội dung đáp án")]
        public string AnswerText { get; set; } = string.Empty;

        [Display(Name = "Đây là đáp án đúng")]
        public bool IsCorrect { get; set; } = false;

        [Display(Name = "Ký hiệu đáp án")]
        public string AnswerLabel { get; set; } = "A"; // A, B, C, D

        // Navigation properties
        public virtual ICollection<TestDetail> TestDetails { get; set; } = new List<TestDetail>();
    }
}
