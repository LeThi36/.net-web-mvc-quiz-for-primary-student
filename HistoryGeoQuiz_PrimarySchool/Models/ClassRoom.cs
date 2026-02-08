using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HistoryGeoQuiz_PrimarySchool.Models
{
    public class ClassRoom
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên lớp")]
        [Display(Name = "Tên lớp")]
        [StringLength(50)]
        public string ClassName { get; set; } = string.Empty; // e.g. "1/1", "5/2"

        [Display(Name = "Khối lớp")]
        public int Grade { get; set; } // 1, 2, 3, 4, 5

        // Foreign Key for Homeroom Teacher (optional provided at creation, can be set later)
        public int? HomeroomTeacherId { get; set; }

        [ForeignKey("HomeroomTeacherId")]
        public virtual User? HomeroomTeacher { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<User> Students { get; set; } = new List<User>();
        public virtual ICollection<TeacherAssignment> TeacherAssignments { get; set; } = new List<TeacherAssignment>();
        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}
