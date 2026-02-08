using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HistoryGeoQuiz_PrimarySchool.Models
{
    public class TeacherAssignment
    {
        public int Id { get; set; }

        [Required]
        public int TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public virtual User? Teacher { get; set; }

        [Required]
        public int ClassRoomId { get; set; }

        [ForeignKey("ClassRoomId")]
        public virtual ClassRoom? ClassRoom { get; set; }

        [Required]
        [Display(Name = "Môn học được phân công")]
        public string Subject { get; set; } = "LichSu"; // "LichSu", "DiaLy", "All"

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    }
}
