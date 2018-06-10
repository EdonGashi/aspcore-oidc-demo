using System.Linq;
using System.Security.Claims;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Identity;

namespace Utils.Authorization
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool HasScope(this ClaimsPrincipal claimsPrincipal, string scope)
        {
            if (claimsPrincipal == null)
            {
                return false;
            }

            var scopes = claimsPrincipal.FindFirstValue(OpenIdConnectConstants.Claims.Scope)?.Split(' ');
            return scopes != null && scopes.Any(s => s == scope);
        }

        public static bool IsCookieAuthenticated(this ClaimsPrincipal claimsPrincipal)
        {
            var identities = claimsPrincipal?.Identities;
            return identities != null && identities.Any(i => i.IsAuthenticated && i.AuthenticationType == IdentityConstants.ApplicationScheme);
        }
    }
}
