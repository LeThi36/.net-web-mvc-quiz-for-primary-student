using HistoryGeoQuiz_PrimarySchool.Enums;

namespace HistoryGeoQuiz_PrimarySchool.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<IReadOnlyList<UploadedFileResult>> UploadManyAsync(
            IEnumerable<Microsoft.AspNetCore.Http.IFormFile> files,
            UploadContext context,
            string ownerUserId,
            CancellationToken ct = default
        );
        
        Task<bool> DeleteAsync(string providerPublicId, string contentType, CancellationToken ct = default);
    }

    public class UploadedFileResult
    {
        public string Url { get; set; } = default!;
        public string FileName { get; set; } = default!;
        public string ContentType { get; set; } = default!;
        public long FileSize { get; set; }
        public FileKind Kind { get; set; }
        public string? ProviderPublicId { get; set; }
    }
}
