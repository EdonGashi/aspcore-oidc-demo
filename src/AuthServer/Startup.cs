using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthServer.Data;
using AuthServer.Infrastructure;
using AuthServer.Models;
using DynamicData.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using Utils.Authorization;
using Utils.Documentation;
using Utils.Security;

namespace AuthServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }

        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var scopes = typeof(AppConstants.Scopes)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                .Select(f => (string)f.GetRawConstantValue())
                .ToList();

            var address = new AddressResolver(Configuration["Tokens:Issuer"]);
            services.AddSingleton<IAddressResolver>(address);
            services.AddSingleton<IScopeCollection>(new ScopeCollection(scopes));

            ConfigureServicesDatabase(services);
            ConfigureServicesMvc(services);
            ConfigureServicesAuth(services, scopes);
            ConfigureServicesApiExplorer(services, scopes);
            ConfigureServicesCookieConsent(services);
        }

        private void ConfigureServicesDatabase(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
                options.UseOpenIddict();
            });

            services.AddDynamicDataStores<ApplicationDbContext, ApplicationUser>();
        }

        private void ConfigureServicesMvc(IServiceCollection services)
        {
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.Configure<RouteOptions>(options => { options.LowercaseUrls = true; });
        }

        private void ConfigureServicesAuth(IServiceCollection services, List<string> scopes)
        {
            var certinfo = new CertificateInfo();
            Configuration.GetSection("Tokens:Certificate").Bind(certinfo);
            var cert = CertificateHelper.LoadCertificate(certinfo, Environment.IsDevelopment());

            services
                .AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 6;
                    options.Password.RequiredUniqueChars = 1;
                    options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                    options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                    options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
                    options.Stores.MaxLengthForKeys = 128;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();

            // Register the OpenIddict services.
            services
                .AddOpenIddict()
                .AddServer(options =>
                {
                    // Register the ASP.NET Core MVC binder used by OpenIddict.
                    // Note: if you don't call this method, you won't be able to
                    // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                    options.UseMvc();

                    // Enable the authorization, logout, token and userinfo endpoints.
                    options
                        .EnableAuthorizationEndpoint("/connect/authorize")
                        .EnableLogoutEndpoint("/connect/logout")
                        .EnableTokenEndpoint("/connect/token")
                        .EnableUserinfoEndpoint("/api/v1/users/me");

                    options.RegisterScopes(scopes.ToArray());

                    // Note: the Mvc.Client sample only uses the code flow and the password flow, but you
                    // can enable the other flows if you need to support implicit or client credentials.
                    options
                        .AllowAuthorizationCodeFlow()
                        .AllowPasswordFlow()
                        .AllowImplicitFlow()
                        .AllowRefreshTokenFlow();

                    // When request caching is enabled, authorization and logout requests
                    // are stored in the distributed cache by OpenIddict and the user agent
                    // is redirected to the same page with a single parameter (request_id).
                    // This allows flowing large OpenID Connect requests even when using
                    // an external authentication provider like Google, Facebook or Twitter.
                    options.EnableRequestCaching();

                    if (Environment.IsDevelopment())
                    {
                        options.DisableHttpsRequirement();
                    }

                    options.UseJsonWebTokens();
                    options.AddSigningCertificate(cert);
                })
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                        .UseDbContext<ApplicationDbContext>();
                });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();
            services
                .AddAuthentication()
                .AddCookie()
                .AddGoogle(options =>
                {
                    options.ClientId = "773091501856-uvv6htap67gve64j05dppf6kii6ues3m.apps.googleusercontent.com";
                    options.ClientSecret = "DNcjBC9fgcY0zUqRe7PYYo1q";
                    options.CallbackPath = "/connect/external/google";
                    options.AccessType = "offline";
                    options.SaveTokens = true;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = true;
                    options.IncludeErrorDetails = true;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = OpenIdConnectConstants.Claims.Subject,
                        RoleClaimType = OpenIdConnectConstants.Claims.Role,
                        ValidateIssuer = true,
                        ValidIssuer = Configuration["Tokens:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = "resource_server",
                        ValidateIssuerSigningKey = false,
                        IssuerSigningKey = new X509SecurityKey(cert),
                        ValidateLifetime = true
                    };
                });

            if (Environment.IsDevelopment())
            {
                services
                    .AddDataProtection()
                    .PersistKeysToFileSystem(GetKeyRingDirInfo())
                    .SetApplicationName("auth_server");
            }
            else
            {
                services
                    .AddDataProtection()
                    .ProtectKeysWithCertificate(Configuration["CookieProtection:Thumbprint"]
                                                ?? throw new InvalidOperationException("Could not find key protection certificate."));
            }

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "auth";
                options.Events.OnRedirectToLogin = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api") &&
                        context.Response.StatusCode == StatusCodes.Status200OK)
                    {
                        context.Response.Clear();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api") &&
                        context.Response.StatusCode == StatusCodes.Status200OK)
                    {
                        context.Response.Clear();
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            services.AddAuthorization();
            services.AddImplicitScopePolicy();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IAuthenticationSchemeProvider, CustomAuthenticationSchemeProvider>();
        }

        private void ConfigureServicesCookieConsent(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
        }

        private void ConfigureServicesApiExplorer(IServiceCollection services, List<string> scopes)
        {
            services.AddMvcCore().AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });

            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "OAuth 2.0 Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });

                options.AddSecurityDefinition("oauth2", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Flow = "implicit",
                    AuthorizationUrl = "/connect/authorize",
                    TokenUrl = "/connect/token",
                    Scopes = scopes.ToDictionary(s => s, s => Configuration.GetScopeTitle(s))
                });

                options.OperationFilter<SecurityRequirementsOperationFilter>();
                options.DocumentFilter<SwaggerUtils.LowercaseDocumentFilter>();

                // resolve the IApiVersionDescriptionProvider service
                // note: that we have to build a temporary service provider here because one has not been created yet
                var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

                // add a swagger document for each discovered API version
                // note: you might choose to skip or document deprecated API versions differently
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(description.GroupName, SwaggerUtils.CreateInfoForApiVersion(description));
                }

                // add a custom operation filter which sets default values
                options.OperationFilter<SwaggerDefaultValues>();

                // integrate xml comments
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IApiVersionDescriptionProvider provider)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.OAuthClientId("swagger");
                options.OAuthAppName("Swagger");
                options.OAuthScopeSeparator(" ");
                options.OAuthUseBasicAuthenticationWithAccessCodeGrant();
                options.OAuthAdditionalQueryStringParams(new { nonce = "swagger" });
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });
        }

        private DirectoryInfo GetKeyRingDirInfo()
        {
            var applicationBasePath = AppContext.BaseDirectory;
            var directoryInfo = new DirectoryInfo(applicationBasePath);
            do
            {
                directoryInfo = directoryInfo.Parent;
                var keyRingDirectoryInfo = new DirectoryInfo(Path.Combine(directoryInfo.FullName, "KeyRing"));
                if (keyRingDirectoryInfo.Exists)
                {
                    return keyRingDirectoryInfo;
                }
            }
            while (directoryInfo.Parent != null);

            throw new Exception($"KeyRing folder could not be located using the application root {applicationBasePath}.");
        }
    }
}
