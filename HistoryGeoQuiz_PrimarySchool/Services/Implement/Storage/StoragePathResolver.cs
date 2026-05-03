using HistoryGeoQuiz_PrimarySchool.Enums;
using HistoryGeoQuiz_PrimarySchool.Options;
using Microsoft.Extensions.Options;

namespace HistoryGeoQuiz_PrimarySchool.Services.Implement.Storage
{
    public class StoragePathResolver
    {
        private readonly StorageOptions _opt;
        public StoragePathResolver(IOptions<StorageOptions> opt) => _opt = opt.Value;

        public string Resolve(UploadContext context, FileKind kind, string ownerUserId)
        {
            string kindFolder = kind switch
            {
                FileKind.Image => _opt.ImagesFolder,
                FileKind.Video => _opt.VideosFolder,
                FileKind.Audio => _opt.AudioFolder,
                FileKind.Document => _opt.DocumentsFolder,
                FileKind.Text => _opt.TextsFolder,
                FileKind.Archive => _opt.ArchivesFolder,
                _ => _opt.FilesFolder
            };

            var ctx = context switch
            {
                UploadContext.Avatar => _opt.Avatars,
                UploadContext.Certificate => _opt.Certificates,
                UploadContext.Material => _opt.Materials,
                UploadContext.IdentityDocument => _opt.IdentityDocuments,
                UploadContext.Chat => _opt.Chat,
                _ => "others"
            };

            return $"{_opt.BaseFolder}/{ctx}/{ownerUserId}/{kindFolder}";
        }

        private static readonly HashSet<string> ImageExts = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg",".jpeg",".png",".gif",".webp",".heic",".heif",".bmp",".tiff",".svg" };

        private static readonly HashSet<string> VideoExts = new(StringComparer.OrdinalIgnoreCase)
        { ".mp4",".mov",".webm",".mkv",".avi" };

        private static readonly HashSet<string> AudioExts = new(StringComparer.OrdinalIgnoreCase)
        { ".mp3",".wav",".aac",".m4a",".flac",".ogg" };

        public static FileKind InferKind(string? contentType, string fileName)
        {
            var ct = (contentType ?? "").ToLowerInvariant();
            var ext = Path.GetExtension(fileName ?? "").ToLowerInvariant();

            if (ct.StartsWith("image/")) return FileKind.Image;
            if (ct.StartsWith("video/")) return FileKind.Video;
            if (ct.StartsWith("audio/")) return FileKind.Audio;
            
            if (ImageExts.Contains(ext)) return FileKind.Image;
            if (VideoExts.Contains(ext)) return FileKind.Video;
            if (AudioExts.Contains(ext)) return FileKind.Audio;

            return FileKind.Raw;
        }
    }
}
