using HistoryGeoQuiz_PrimarySchool.ViewModels;

namespace HistoryGeoQuiz_PrimarySchool.Services.Interfaces
{
    public interface IProfileService
    {
        Task<ProfileViewModel?> GetProfileAsync(Guid userId);
        Task<(bool Success, string? Error)> UpdateProfileAsync(Guid userId, UpdateProfileViewModel model);
    }
}
