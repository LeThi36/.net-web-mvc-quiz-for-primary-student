using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HistoryGeoQuiz_PrimarySchool.Models
{
    public class TestDetail : BaseEntity
    {
        public Guid TestResultId { get; set; }

        [ForeignKey("TestResultId")]
        public virtual TestResult? TestResult { get; set; }

        public Guid QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question? Question { get; set; }

        public Guid? SelectedAnswerId { get; set; }

        [ForeignKey("SelectedAnswerId")]
        public virtual Answer? SelectedAnswer { get; set; }

        [Display(Name = "Trả lời đúng")]
        public bool IsCorrect { get; set; }
    }
}
