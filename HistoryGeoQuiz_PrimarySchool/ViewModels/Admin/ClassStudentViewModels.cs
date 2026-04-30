using System.ComponentModel.DataAnnotations;

namespace HistoryGeoQuiz_PrimarySchool.ViewModels
{
    /// <summary>
    /// ViewModel for adding a new student to a class (creates account)
    /// </summary>
    public class AddStudentToClassViewModel
    {
        public Guid ClassRoomId { get; set; }
        public string? ClassName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và tên")]
        [StringLength(100, ErrorMessage = "Họ tên không được quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3-50 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Tên đăng nhập chỉ được chứa chữ cái, số và dấu gạch dưới")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [Display(Name = "Mật khẩu")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Mật khẩu phải từ 4-100 ký tự")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Display(Name = "Xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel for importing students from Excel
    /// </summary>
    public class ImportStudentsViewModel
    {
        public Guid ClassRoomId { get; set; }
        public string? ClassName { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn file Excel")]
        [Display(Name = "File Excel (.xlsx)")]
        public IFormFile? ExcelFile { get; set; }
    }

    /// <summary>
    /// Result of importing students from Excel
    /// </summary>
    public class ImportStudentsResultViewModel
    {
        public Guid ClassRoomId { get; set; }
        public string? ClassName { get; set; }
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<ImportStudentError> Errors { get; set; } = new List<ImportStudentError>();
        public List<ImportStudentSuccess> SuccessfulStudents { get; set; } = new List<ImportStudentSuccess>();
    }

    public class ImportStudentError
    {
        public int RowNumber { get; set; }
        public string? FullName { get; set; }
        public string? Username { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class ImportStudentSuccess
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel for transferring a student to another class
    /// </summary>
    public class TransferStudentViewModel
    {
        public Guid StudentId { get; set; }
        public string? StudentName { get; set; }
        public Guid CurrentClassRoomId { get; set; }
        public string? CurrentClassName { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn lớp đích")]
        [Display(Name = "Chuyển đến lớp")]
        public Guid TargetClassRoomId { get; set; }

        public List<ClassRoomSelectItem> AvailableClasses { get; set; } = new List<ClassRoomSelectItem>();
    }

    public class ClassRoomSelectItem
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = string.Empty; // e.g. "Lớp 1/1 - Khối 1"
    }

    /// <summary>
    /// ViewModel for assigning an existing student (without class) to a class
    /// </summary>
    public class AssignExistingStudentViewModel
    {
        public Guid ClassRoomId { get; set; }
        public string? ClassName { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn học sinh")]
        [Display(Name = "Chọn học sinh")]
        public Guid StudentId { get; set; }

        public List<StudentSelectItem> AvailableStudents { get; set; } = new List<StudentSelectItem>();
    }

    public class StudentSelectItem
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string DisplayName => $"{FullName} ({Username})";
    }
}
