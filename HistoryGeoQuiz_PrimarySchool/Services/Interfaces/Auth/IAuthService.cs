using HistoryGeoQuiz_PrimarySchool.Models;

namespace HistoryGeoQuiz_PrimarySchool.Services.Interfaces
{
    public interface IAuthService
    {
        /// <summary>Authenticate user by username and password. Returns null if invalid.</summary>
        Task<User?> LoginAsync(string username, string password);

        /// <summary>Register a new user. Returns (success, errorMessage).</summary>
        Task<(bool Success, string? Error)> RegisterAsync(string username, string password, string fullName, HistoryGeoQuiz_PrimarySchool.Enums.UserRole role);

        /// <summary>Check if a username already exists.</summary>
        Task<bool> UsernameExistsAsync(string username);
    }
}
