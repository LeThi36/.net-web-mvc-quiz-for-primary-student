using Microsoft.AspNetCore.Mvc;
using HistoryGeoQuiz_PrimarySchool.Constants;
using HistoryGeoQuiz_PrimarySchool.Enums;

namespace HistoryGeoQuiz_PrimarySchool.Controllers
{
    /// <summary>
    /// Base controller providing common session helper methods.
    /// All controllers should inherit from this instead of Controller directly.
    /// </summary>
    public abstract class BaseController : Controller
    {
        /// <summary>Get current logged-in user's ID from session.</summary>
        protected Guid GetCurrentUserId()
        {
            var userIdStr = HttpContext.Session.GetString(SessionKeys.UserId);
            return Guid.TryParse(userIdStr, out var userId) ? userId : Guid.Empty;
        }

        /// <summary>Get current logged-in user's role from session.</summary>
        protected string? GetCurrentUserRole() =>
            HttpContext.Session.GetString(SessionKeys.UserRole);

        /// <summary>Get current logged-in user's display name from session.</summary>
        protected string? GetCurrentUserName() =>
            HttpContext.Session.GetString(SessionKeys.UserName);

        /// <summary>Check if current user has the specified role.</summary>
        protected bool IsInRole(UserRole role) =>
            Enum.TryParse<UserRole>(GetCurrentUserRole(), out var sessionRole) && sessionRole == role;
    }
}
