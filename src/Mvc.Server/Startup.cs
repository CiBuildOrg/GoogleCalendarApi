using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Mvc.Server.DataObjects.Configuration;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using Mvc.Server.Infrastructure.Attributes;
using Mvc.Server.Infrastructure.Security;
using Mvc.Server.Infrastructure.Utils;
using OpenIddict.Core;
using OwaspHeaders.Core.Extensions;
using OwaspHeaders.Core.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Mvc.Server.Infrastructure.Filters;
using Microsoft.AspNetCore.Authorization;

namespace Mvc.Server
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("config.json", true, true)
                .AddJsonFile($"config.{env.EnvironmentName.ToLower()}.json", true)
                .AddEnvironmentVariables();
            Configuration = configuration.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<AppOptions>(options => Core.Utilities.Configuration.ConfigurationBinder.Bind(Configuration, options));
            services.AddSingleton<IConfiguration>(Configuration);

            //// Add Swagger generator
            //services.AddSwaggerGen(options =>
            //{
            //    options.SwaggerDoc("v1", new Info { Title = "Api Starter", Version = "v1" });
            //});

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            //services.AddAuthentication(options =>
            //    {
            //        options.DefaultScheme = OAuthIntrospectionDefaults.AuthenticationScheme;
            //    })

            //    .AddOAuthIntrospection(options =>
            //    {
            //        options.Authority = new Uri("http://localhost:5001/");
            //        options.Audiences.Add("mvc");
            //        options.ClientId = "mvc";
            //        options.ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654";
            //        options.RequireHttpsMetadata = false;

            //        // Note: you can override the default name and role claims:
            //        // options.NameClaimType = "custom_name_claim";
            //        // options.RoleClaimType = "custom_role_claim";
            //    });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })

            .AddCookie(options =>
            {
                options.LoginPath = new PathString("/signin");
            })

                .AddOpenIdConnect(options =>
                {
                    // Note: these settings must match the application details
                    // inserted in the database at the server level.
                    options.ClientId = "mvc";
                    options.ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654";
                    options.RequireHttpsMetadata = false;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;

                    // Use the authorization code flow.
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;
                    // Note: setting the Authority allows the OIDC client middleware to automatically
                    // retrieve the identity provider's configuration and spare you from setting
                    // the different endpoints URIs or the token validation parameters explicitly.
                    options.Authority = "http://localhost:5001/";

                    options.Scope.Add(OpenIdConnectConstants.Scopes.OpenId);
                    options.Scope.Add(OpenIdConnectConstants.Scopes.Email);
                    options.Scope.Add(OpenIdConnectConstants.Scopes.Profile);
                    options.Scope.Add(OpenIdConnectConstants.Scopes.OfflineAccess);
                    options.Scope.Add(OpenIddictConstants.Scopes.Roles);
                });

            services.AddAuthorization(options =>
             {
                 // Create a policy for each permission
                 foreach (var permissionClaim in PermissionClaims.GetAll())
                 {
                     options.AddPolicy(permissionClaim, policy => policy.Requirements.Add(new PermissionRequirement(permissionClaim)));
                 }
             });

            services
                .AddMvc();

            services.AddScoped<IAuthorizationHandler, PermissionHandler>();

            services.AddSingleton<HttpClient>();
            services.Configure<SecureHeadersMiddlewareConfiguration>(
               Configuration.GetSection("SecureHeadersMiddlewareConfiguration"));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            IOptions<SecureHeadersMiddlewareConfiguration> secureHeaderSettings)
        {
            loggerFactory.AddSerilog();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();


            app.UseSecureHeadersMiddleware(secureHeaderSettings.Value);

            //// Enable middleware to serve generated Swagger as a JSON endpoint.
            //app.UseSwagger();
            //// Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
            //app.UseSwaggerUI(c =>
            //{
            //    c.RoutePrefix = "apidocs";
            //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My Web API");
            //});
        }
    }
}
