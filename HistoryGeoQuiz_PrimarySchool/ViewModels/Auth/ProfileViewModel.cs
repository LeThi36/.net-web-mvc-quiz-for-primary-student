using HistoryGeoQuiz_PrimarySchool.Enums;
using System.ComponentModel.DataAnnotations;

namespace HistoryGeoQuiz_PrimarySchool.ViewModels.Auth
{
    public class ProfileViewModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public UserRole Role { get; set; }
        public Gender? Gender { get; set; }
        public string? ClassName { get; set; }
    }

    public class UpdateProfileViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và tên")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        [Display(Name = "Giới tính")]
        public Gender? Gender { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public Microsoft.AspNetCore.Http.IFormFile? AvatarFile { get; set; }
    }
}
