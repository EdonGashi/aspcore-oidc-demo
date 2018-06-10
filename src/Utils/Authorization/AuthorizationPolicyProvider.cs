using System.Threading.Tasks;
using AspNet.Security.OAuth.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Utils.Authorization
{
    public class AuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        private readonly IConfiguration configuration;

        public AuthorizationPolicyProvider(IOptions<AuthorizationOptions> options, IConfiguration configuration)
            : base(options)
        {
            this.configuration = configuration;
        }

        public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            var policy = await base.GetPolicyAsync(policyName);
            if (policy != null)
            {
                return policy;
            }

            var requirement = new HasScopeRequirement(policyName, /* TODO: Provide iss domain */null);
            var builder = new AuthorizationPolicyBuilder()
                .AddRequirements(requirement);

            if (requirement.Strict)
            {
                builder.AddAuthenticationSchemes(OAuthValidationDefaults.AuthenticationScheme);
            }

            return builder.Build();
        }
    }
}
