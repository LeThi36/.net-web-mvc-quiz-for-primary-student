namespace HistoryGeoQuiz_PrimarySchool.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string? Message { get; set; }

        public string? StatusCode { get; set; }

        public string? StackTrace { get; set; }

        public string? Path { get; set; }

        public bool IsDevelopment { get; set; }
    }
}
