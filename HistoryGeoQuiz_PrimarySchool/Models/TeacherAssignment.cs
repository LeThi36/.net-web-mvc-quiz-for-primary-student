using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HistoryGeoQuiz_PrimarySchool.Helpers;

namespace HistoryGeoQuiz_PrimarySchool.Models
{
    public class TeacherAssignment : BaseEntity
    {
        [Required]
        public Guid TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public virtual User? Teacher { get; set; }

        [Required]
        public Guid ClassRoomId { get; set; }

        [ForeignKey("ClassRoomId")]
        public virtual ClassRoom? ClassRoom { get; set; }

        [Required]
        [Display(Name = "Môn học được phân công")]
        public string Subject { get; set; }

        public DateTime AssignedDate { get; set; } = DateTimeHelper.GetVietnamTime();
    }
}
