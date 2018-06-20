using System;
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

            var scopes = context.User.Claims
                .Where(c => c.Type == OpenIdConnectConstants.Claims.Scope && (requirement.Issuer == null || c.Issuer == requirement.Issuer))
                .SelectMany(s => s.Value?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>());

            if (scopes.Any(requirement.Scope.Contains))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}