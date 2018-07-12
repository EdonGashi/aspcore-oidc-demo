using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.Extensions.Configuration;
using OpenIddict.Abstractions;

namespace AuthServer
{
    public static class AppConstants
    {
        public static string GetScopeTitle(this IConfiguration configuration, string scope)
        {
            return GetScopeTitle(configuration, scope, "en");
        }

        public static string GetScopeTitle(this IConfiguration configuration, string scope, string locale)
        {
            return configuration[$"Scopes:{locale}:{scope}:title"] ?? scope;
        }

        public static class Scopes
        {
            public const string OpenId = OpenIdConnectConstants.Scopes.OpenId;

            public const string Email = OpenIdConnectConstants.Scopes.Email;

            public const string Profile = OpenIdConnectConstants.Scopes.Profile;

            public const string Phone = OpenIdConnectConstants.Scopes.Phone;

            public const string Roles = OpenIddictConstants.Scopes.Roles;

            public const string ApplicationRead = "app.read";

            public const string ApplicationWrite = "app.write";

            public const string ValuesRead = "values.read";

            public const string ValuesWrite = "values.write";
        }

        public static class Roles
        {
            public const string Application = "application";

            public const string Administrator = "administrator";

            public const string Teacher = "teacher";

            public const string Student = "student";
        }

        public static string AdminEmail = "admin@authserver";
    }
}
