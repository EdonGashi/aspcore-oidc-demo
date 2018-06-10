using System;
using Microsoft.AspNetCore.Authorization;

namespace Utils.Authorization
{
    public class HasScopeRequirement : IAuthorizationRequirement
    {
        public HasScopeRequirement(string scope, string issuer)
        {
            if (scope.StartsWith('!'))
            {
                scope = scope.Substring(1);
                Strict = true;
            }

            Scope = (scope ?? throw new ArgumentNullException(nameof(scope)))
                .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            Issuer = issuer;
        }

        public string[] Scope { get; }

        public string Issuer { get; }

        public bool Strict { get; }
    }
}
