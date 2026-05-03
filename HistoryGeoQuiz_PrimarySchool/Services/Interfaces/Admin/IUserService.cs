using Microsoft.AspNetCore.Http;
using HistoryGeoQuiz_PrimarySchool.Models;
using HistoryGeoQuiz_PrimarySchool.ViewModels;
using HistoryGeoQuiz_PrimarySchool.ViewModels.Admin;

namespace HistoryGeoQuiz_PrimarySchool.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetTeachersAsync();
        Task<ImportTeachersResultViewModel> ImportTeachersFromExcelAsync(IFormFile file);
        Task<(bool Success, string? Error)> CreateTeacherAsync(CreateTeacherViewModel model);
    }
}
