using System;
using System.Threading.Tasks;
using AuthServer.Data;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using Utils.Initialization;

namespace AuthServer.Infrastructure.Initialization
{
    public class ClientInitializer : IStartupService
    {
        private static string Combine(string address, string path)
        {
            if (path.StartsWith("/"))
            {
                if (address.EndsWith("/"))
                {
                    return address + path.Substring(1);
                }

                return address + path;
            }

            if (address.EndsWith("/"))
            {
                return address + path;
            }

            return address + "/" + path;
        }

        private readonly IAddressResolver addressResolver;
        private readonly ApplicationDbContext context;
        private readonly OpenIddictApplicationManager<OpenIddictApplication> manager;
        private readonly IScopeCollection scopes;
        private readonly IConfiguration configuration;

        public ClientInitializer(
            IAddressResolver addressResolver,
            ApplicationDbContext context,
            OpenIddictApplicationManager<OpenIddictApplication> manager,
            IScopeCollection scopes,
            IConfiguration configuration)
        {
            this.addressResolver = addressResolver;
            this.context = context;
            this.manager = manager;
            this.scopes = scopes;
            this.configuration = configuration;
        }

        public async Task InitializeAsync()
        {
            //if (await manager.FindByClientIdAsync("react") == null)
            //{
            //    var descriptor = new OpenIddictApplicationDescriptor
            //    {
            //        ClientId = "react",
            //        ClientSecret = "react_secret",
            //        DisplayName = "SPA client application",
            //        PostLogoutRedirectUris = { new Uri("http://localhost:3000/signout-callback-oidc") },
            //        RedirectUris = { new Uri("http://localhost:3000/signin-oidc") },
            //        Permissions =
            //        {
            //            OpenIddictConstants.Permissions.Endpoints.Authorization,
            //            OpenIddictConstants.Permissions.Endpoints.Logout,
            //            OpenIddictConstants.Permissions.GrantTypes.Implicit
            //        }
            //    };

            //    await manager.CreateAsync(descriptor);
            //}

            var resourceServer = await manager.FindByClientIdAsync("resource_server");
            if (resourceServer == null)
            {
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = "resource_server",
                    ClientSecret = "resource_server_secret",
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.GrantTypes.ClientCredentials
                    }
                };

                await manager.CreateAsync(descriptor);
            }

            var mvcClient = await manager.FindByClientIdAsync("mvc_client");
            if (mvcClient == null)
            {
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = "mvc_client",
                    ClientSecret = "mvc_client_secret",
                    DisplayName = "MVC Client application",
                    PostLogoutRedirectUris = { new Uri($"{configuration["Client:BaseUrl"]}external/logout") },
                    RedirectUris = { new Uri($"{configuration["Client:BaseUrl"]}external/login") },
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Authorization,
                        OpenIddictConstants.Permissions.Endpoints.Logout,
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                        OpenIddictConstants.Permissions.GrantTypes.RefreshToken
                    }
                };

                foreach (var scope in scopes.GetScopes())
                {
                    descriptor.Permissions.Add($"scp:{scope}");
                }

                await manager.CreateAsync(descriptor);
            }
            else
            {
                var permissions = new JArray(
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Logout,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken);

                foreach (var scope in scopes.GetScopes())
                {
                    permissions.Add($"scp:{scope}");
                }

                mvcClient.Permissions = JsonConvert.SerializeObject(permissions);
                await manager.UpdateAsync(mvcClient);
            }

            var swagger = await manager.FindByClientIdAsync("swagger");
            if (swagger == null)
            {
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = "swagger",
                    DisplayName = "Swagger",
                    RedirectUris = { new Uri(Combine(addressResolver.Resolve(), "/swagger/oauth2-redirect.html")) },
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Authorization,
                        OpenIddictConstants.Permissions.GrantTypes.Implicit
                    }
                };

                foreach (var scope in scopes.GetScopes())
                {
                    descriptor.Permissions.Add($"scp:{scope}");
                }

                await manager.CreateAsync(descriptor);
            }
            else
            {
                var permissions = new JArray(OpenIddictConstants.Permissions.Endpoints.Authorization, OpenIddictConstants.Permissions.GrantTypes.Implicit);
                foreach (var scope in scopes.GetScopes())
                {
                    permissions.Add($"scp:{scope}");
                }

                swagger.Permissions = JsonConvert.SerializeObject(permissions);
                await manager.UpdateAsync(swagger);
            }
        }
    }
}
