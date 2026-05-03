using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using HistoryGeoQuiz_PrimarySchool.Constants;
using HistoryGeoQuiz_PrimarySchool.Enums;

namespace HistoryGeoQuiz_PrimarySchool.Filters
{
    /// <summary>
    /// Custom authorization filter that checks user role from session.
    /// Replaces duplicated IsTeacher(), IsAdmin(), IsStudent() methods in controllers.
    /// 
    /// Usage:
    ///   [AuthorizeRole(UserRole.Teacher)]
    ///   [AuthorizeRole(UserRole.Admin, UserRole.Teacher)]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly UserRole[] _roles;

        public AuthorizeRoleAttribute(params UserRole[] roles)
        {
            _roles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            var hasRole = _roles.Any(r => user.IsInRole(r.ToString()));

            if (!hasRole)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
