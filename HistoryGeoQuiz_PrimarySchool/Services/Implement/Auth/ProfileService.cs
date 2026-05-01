using HistoryGeoQuiz_PrimarySchool.Enums;
using HistoryGeoQuiz_PrimarySchool.Helpers;
using HistoryGeoQuiz_PrimarySchool.Models;
using HistoryGeoQuiz_PrimarySchool.Repositories.Interfaces;
using HistoryGeoQuiz_PrimarySchool.Services.Interfaces;
using HistoryGeoQuiz_PrimarySchool.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HistoryGeoQuiz_PrimarySchool.Services.Implement.Auth
{
    public class ProfileService : IProfileService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRepository<Media> _mediaRepo;
        private readonly IFileStorageService _storageService;
        private readonly ILogger<ProfileService> _logger;

        public ProfileService(
            IUserRepository userRepo, 
            IRepository<Media> mediaRepo,
            IFileStorageService storageService,
            ILogger<ProfileService> logger)
        {
            _userRepo = userRepo;
            _mediaRepo = mediaRepo;
            _storageService = storageService;
            _logger = logger;
        }

        public async Task<ProfileViewModel?> GetProfileAsync(Guid userId)
        {
            var user = await _userRepo.GetStudentWithClassInfoAsync(userId);
            if (user == null) return null;

            return new ProfileViewModel
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                AvatarUrl = user.Avatar?.Url ?? AvatarHelper.ForUser(user.Id.ToString(), user.Gender),
                Role = user.Role,
                Gender = user.Gender,
                ClassName = user.ClassRoom?.ClassName
            };
        }

        public async Task<(bool Success, string? Error)> UpdateProfileAsync(Guid userId, UpdateProfileViewModel model)
        {
            // Fetch user with Avatar included
            var user = await _userRepo.FindAsync(u => u.Id == userId);
            var userEntity = user.FirstOrDefault();
            if (userEntity == null) return (false, "Người dùng không tồn tại.");

            // Manually load Avatar because generic repository doesn't include
            // We can do this because we have access to the context in the repository if we were using it, 
            // but here we just fetch it separately if needed or update the repo.
            // For simplicity, let's just fetch the avatar record if AvatarId is set.
            Media? oldAvatar = null;
            if (userEntity.AvatarId.HasValue)
            {
                oldAvatar = await _mediaRepo.GetByIdAsync(userEntity.AvatarId.Value);
            }

            try
            {
                // [Business Rule] Students can only change avatar
                if (userEntity.Role == UserRole.Teacher || userEntity.Role == UserRole.Admin)
                {
                    userEntity.FullName = model.FullName;
                    if (model.Gender.HasValue)
                    {
                        userEntity.Gender = model.Gender.Value;
                    }
                }

                if (model.AvatarFile != null)
                {
                    AvatarHelper.ValidateAvatarFile(model.AvatarFile);

                    // Upload new avatar
                    var uploadResults = await _storageService.UploadManyAsync(
                        new[] { model.AvatarFile },
                        UploadContext.Avatar,
                        userEntity.Id.ToString()
                    );

                    if (uploadResults.Any())
                    {
                        var res = uploadResults[0];
                        
                        // Create new Media record
                        var newMedia = new Media
                        {
                            Url = res.Url,
                            FileName = res.FileName,
                            ContentType = res.ContentType,
                            FileSize = res.FileSize,
                            ProviderPublicId = res.ProviderPublicId,
                            UploadedByUserId = userEntity.Id
                        };

                        await _mediaRepo.AddAsync(newMedia);
                        await _mediaRepo.SaveChangesAsync();

                        // Delete old avatar from Cloudinary and DB
                        if (oldAvatar != null)
                        {
                            if (!string.IsNullOrEmpty(oldAvatar.ProviderPublicId))
                            {
                                await _storageService.DeleteAsync(oldAvatar.ProviderPublicId, oldAvatar.ContentType ?? "image/png");
                            }
                            _mediaRepo.Remove(oldAvatar);
                            // We don't save yet, will save with user update
                        }

                        userEntity.AvatarId = newMedia.Id;
                    }
                }

                _userRepo.Update(userEntity);
                await _userRepo.SaveChangesAsync();

                _logger.LogInformation("Profile updated for user {UserId}", userId);
                return (true, null);
            }
            catch (ArgumentException ex)
            {
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
                return (false, "Đã xảy ra lỗi khi cập nhật trang cá nhân.");
            }
        }
    }
}
