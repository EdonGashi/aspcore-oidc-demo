using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Utils.Documentation
{
    public class SecurityRequirementsOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var authorizeAttributes = context.ApiDescription
                .ActionAttributes()
                .OfType<AuthorizeAttribute>()
                .ToList();

            if (context.ApiDescription.ActionAttributes().OfType<AllowAnonymousAttribute>().Any())
            {
                return;
            }

            authorizeAttributes.AddRange(context.ApiDescription.ControllerAttributes().OfType<AuthorizeAttribute>());

            if (authorizeAttributes.Count != 0)
            {
                operation.Responses.Add("401", new Response { Description = "Unauthorized" });

                var roles = authorizeAttributes
                    .Where(attr => !string.IsNullOrEmpty(attr.Roles))
                    .SelectMany(r => r.Roles.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    .ToList();

                var scopes = authorizeAttributes
                    .Where(attr => !string.IsNullOrEmpty(attr.Policy))
                    .SelectMany(r => r.Policy.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    .ToList();

                if (roles.Count != 0 || scopes.Count != 0)
                {
                    operation.Responses.Add("403", new Response { Description = "Forbidden" });
                    var authorizationDescription = new StringBuilder(" (Auth");
                    if (roles.Count != 0)
                    {
                        authorizationDescription.Append($" roles: {string.Join(", ", roles)};");
                    }

                    if (scopes.Count != 0)
                    {
                        authorizationDescription.Append($" scopes: {string.Join(", ", scopes)};");
                    }

                    operation.Summary += authorizationDescription.ToString().TrimEnd(';') + ")";
                }

                operation.Security = new List<IDictionary<string, IEnumerable<string>>>
                {
                    new Dictionary<string, IEnumerable<string>>
                    {
                        ["Bearer"] = new string[] { },
                        ["oauth2"] = scopes
                    }
                };
            }
        }
    }
}
