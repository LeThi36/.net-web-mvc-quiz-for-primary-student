using System.ComponentModel.DataAnnotations;

namespace HistoryGeoQuiz_PrimarySchool.Models
{
    public class Media : BaseEntity
    {
        [Required]
        public string Url { get; set; } = string.Empty;

        public string? FileName { get; set; }

        public string? ContentType { get; set; }

        public long FileSize { get; set; }

        public string? ProviderPublicId { get; set; }

        public Guid? UploadedByUserId { get; set; }
    }
}
