using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using Utils.Authorization;

namespace Utils.Mvc
{
    public sealed class CookieAuthenticationRequiredAttribute : ActionMethodSelectorAttribute
    {
        public override bool IsValidForRequest(RouteContext context, ActionDescriptor action)
        {
            return context.HttpContext.User.IsCookieAuthenticated();
        }
    }
}
