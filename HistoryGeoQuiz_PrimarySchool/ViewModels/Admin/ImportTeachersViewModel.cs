using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace HistoryGeoQuiz_PrimarySchool.ViewModels.Admin
{
    public class ImportTeachersViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn file Excel")]
        [Display(Name = "File Excel danh sách giáo viên")]
        public IFormFile ExcelFile { get; set; } = null!;
    }
}
