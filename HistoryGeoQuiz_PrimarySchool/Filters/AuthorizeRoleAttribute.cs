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
            var roleStr = context.HttpContext.Session.GetString(SessionKeys.UserRole);

            if (string.IsNullOrEmpty(roleStr) || 
                !Enum.TryParse<UserRole>(roleStr, out var userRole) || 
                !_roles.Contains(userRole))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
