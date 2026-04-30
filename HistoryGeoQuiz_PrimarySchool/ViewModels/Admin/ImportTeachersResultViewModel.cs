namespace HistoryGeoQuiz_PrimarySchool.ViewModels
{
    public class ImportTeachersResultViewModel
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<ImportTeacherSuccess> SuccessfulTeachers { get; set; } = new();
        public List<ImportTeacherError> Errors { get; set; } = new();
    }

    public class ImportTeacherSuccess
    {
        public string? FullName { get; set; }
        public string? Username { get; set; }
    }

    public class ImportTeacherError
    {
        public int RowNumber { get; set; }
        public string? FullName { get; set; }
        public string? Username { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
