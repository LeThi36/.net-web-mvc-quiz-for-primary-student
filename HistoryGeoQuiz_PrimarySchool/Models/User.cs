using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HistoryGeoQuiz_PrimarySchool.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và tên")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Vai trò")]
        public string Role { get; set; } = "Student"; // "Teacher" or "Student"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // For Students: Which class are they in?
        [Display(Name = "Lớp")]
        public int? ClassRoomId { get; set; }

        [ForeignKey("ClassRoomId")]
        public virtual ClassRoom? ClassRoom { get; set; }

        // Navigation properties
        public virtual ICollection<Lesson> CreatedLessons { get; set; } = new List<Lesson>();
        public virtual ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
        
        // Fore Teacher: Assignments
        public virtual ICollection<TeacherAssignment> TeacherAssignments { get; set; } = new List<TeacherAssignment>();
    }
}
