using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using AspNet.Security.OpenIdConnect.Primitives;
using AuthServer.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using Utils.Authorization;
using Utils.Documentation;

namespace ResourceServer
{
    public class Startup
    {
        private static string Join(string domain, string path)
        {
            if (domain.EndsWith("/"))
            {
                domain = domain.Substring(0, domain.Length - 1);
            }

            if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }

            return domain + "/" + path;
        }

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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();
            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = Configuration["Tokens:Issuer"];
                    options.Audience = "resource_server";
                    options.RequireHttpsMetadata = !Environment.IsDevelopment();
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
                        RequireSignedTokens = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true
                    };
                });

            services.AddAuthorization();
            services.AddImplicitScopePolicy();
            var httpClient = new HttpClient();
            services.AddSingleton(httpClient);
            services.AddSingleton<ITokenProvider>(new ClientCredentialsAuthenticator(
                "resource_server",
                "resource_server_secret",
                httpClient,
                Join(Configuration["Tokens:Issuer"], "/connect/token")));

            ConfigureServicesApiExplorer(services, new List<string>
            {
                "openid",
                "profile",
                "email",
                "values"
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

                options.OperationFilter<SecurityRequirementsOperationFilter>();
                options.DocumentFilter<LowercaseDocumentFilter>();

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
            IHostingEnvironment env,
            IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });
        }
    }
}
