using Microsoft.AspNetCore.Mvc;
using HistoryGeoQuiz_PrimarySchool.Enums;
using System.Security.Claims;

namespace HistoryGeoQuiz_PrimarySchool.Controllers
{
    /// <summary>
    /// Base controller providing common session helper methods.
    /// All controllers should inherit from this instead of Controller directly.
    /// </summary>
    public abstract class BaseController : Controller
    {
        /// <summary>Get current logged-in user's ID from Claims.</summary>
        protected Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;
        }

        /// <summary>Get current logged-in user's role from Claims.</summary>
        protected string? GetCurrentUserRole() =>
            User.FindFirst(ClaimTypes.Role)?.Value;

        /// <summary>Get current logged-in user's display name from Claims.</summary>
        protected string? GetCurrentUserName() =>
            User.FindFirst("FullName")?.Value;

        /// <summary>Check if current user has the specified role.</summary>
        protected bool IsInRole(UserRole role) =>
            User.IsInRole(role.ToString());
    }
}
