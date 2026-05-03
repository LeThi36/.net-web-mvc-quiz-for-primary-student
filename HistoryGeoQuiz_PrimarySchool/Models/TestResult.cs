using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HistoryGeoQuiz_PrimarySchool.Helpers;

namespace HistoryGeoQuiz_PrimarySchool.Models
{
    public class TestResult : BaseEntity
    {
        public Guid StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual User? Student { get; set; }

        public Guid LessonId { get; set; }

        [ForeignKey("LessonId")]
        public virtual Lesson? Lesson { get; set; }

        [Display(Name = "Tổng số câu hỏi")]
        public int TotalQuestions { get; set; }

        [Display(Name = "Số câu đúng")]
        public int CorrectAnswers { get; set; }

        [Display(Name = "Điểm số")]
        public double Score { get; set; }

        [Display(Name = "Đạt/Không đạt")]
        public bool IsPassed { get; set; }

        [Display(Name = "Thời gian hoàn thành")]
        public DateTime CompletedAt { get; set; } = DateTimeHelper.GetVietnamTime();

        [Display(Name = "Thời gian làm bài (giây)")]
        public int TimeTakenSeconds { get; set; }

        // Navigation properties
        public virtual ICollection<TestDetail> Details { get; set; } = new List<TestDetail>();
    }
}
