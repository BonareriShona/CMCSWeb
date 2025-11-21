using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace CMCSWeb.Filters
{
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _role;

        public AuthorizeRoleAttribute(string role)
        {
            _role = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Get role from session
            var currentRole = context.HttpContext.Session.GetString("UserRole");

            // If no session = not logged in
            if (string.IsNullOrEmpty(currentRole))
            {
                context.Result = new RedirectToActionResult("Login", "Account",
                    new { returnUrl = context.HttpContext.Request.Path });
                return;
            }

            // If logged-in user has the wrong role = forbid
            if (!currentRole.Equals(_role, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            }
        }
    }
}