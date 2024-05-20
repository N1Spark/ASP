using ASP.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace ASP.Controllers
{
    public class BackendController : ControllerBase, IActionFilter
    {
        protected bool isAuthenticated;
        protected bool isAdmin;
        protected IEnumerable<Claim>? claims;

        [NonAction]
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var identity = User.Identities
                .FirstOrDefault(i => i.AuthenticationType == nameof(AuthSessionMiddleware));

            identity ??= User.Identities
                .FirstOrDefault(i => i.AuthenticationType == nameof(AuthTokenMiddleware));

            this.isAuthenticated = identity != null;

            String? userRole = identity?.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            this.isAdmin = "Admin".Equals(userRole);

            claims = identity?.Claims;
        }

        [NonAction]
        public void OnActionExecuted(ActionExecutedContext context)
        { }

        protected String? GetAdminAuthMessage()
        {
            if (!isAuthenticated)
            {
                // якщо авторизація не пройдена, то повідомлення в Items
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return HttpContext.Items[nameof(AuthTokenMiddleware)]?.ToString() ?? "Auth required";
            }
            if (!isAdmin)
            {
                Response.StatusCode = StatusCodes.Status403Forbidden;
                return "Access to API forbidden";
            }
            return null;
        }
    }

}
