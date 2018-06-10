using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authorization;

namespace Utils.Authorization
{
    public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            HasScopeRequirement requirement)
        {
            if (!requirement.Strict && context.User.IsCookieAuthenticated())
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // If user does not have the scope claim, get out of here
            if (!context.User.HasClaim(c => c.Type == OpenIdConnectConstants.Claims.Scope && c.Issuer == requirement.Issuer))
            {
                return Task.CompletedTask;
            }

            // Split the scopes string into an array
            var scopes = context
                .User.FindFirst(c => c.Type == OpenIdConnectConstants.Claims.Scope && (requirement.Issuer == null || c.Issuer == requirement.Issuer))
                .Value.Split(' ');

            if (scopes.Any(requirement.Scope.Contains))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}