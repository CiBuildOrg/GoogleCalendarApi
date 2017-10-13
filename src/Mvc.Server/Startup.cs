using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Mvc.Server.DataObjects.Configuration;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using Mvc.Server.Infrastructure.Attributes;
using Mvc.Server.Infrastructure.Security;
using Mvc.Server.Infrastructure.Utils;
using OpenIddict.Core;

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
            var opts = Core.Utilities.Configuration.ConfigurationBinder.Get<AppOptions>(Configuration);

            // Add Swagger generator
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info { Title = "Api Starter", Version = "v1" });
            });

            ////Add MVC Core
            //services.AddMvcCore(
            //        options =>
            //        {
            //            //// Add global authorization filter 
            //            //var policy = new AuthorizationPolicyBuilder()
            //            //    .RequireAuthenticatedUser()
            //            //    .Build();
            
            //            //options.Filters.Add(new ApplicationAuthorizeFilter(policy));

            //            // Add global exception handler for production
            //            options.Filters.Add(typeof(CustomExceptionFilterAttribute));

            //            // Add global validation filter
            //            options.Filters.Add(typeof(ValidateModelFilterAttribute));

            //        }
            //    )
            //    .AddJsonFormatters()
            //    .AddAuthorization(options =>
            //    {
            //        // Create a policy for each permission
            //        foreach (var permissionClaim in PermissionClaims.GetAll())
            //        {
            //            options.AddPolicy(permissionClaim, policy => policy.Requirements.Add(new PermissionRequirement(permissionClaim)));
            //        }
            //    })
            //    .AddDataAnnotations()
            //    .AddCors()
            //    .AddApiExplorer().ConfigureApplicationPartManager(manager =>
            //    {
            //        var oldMetadataReferenceFeatureProvider = manager.FeatureProviders.FirstOrDefault(f => f is MetadataReferenceFeatureProvider);
            //        if (oldMetadataReferenceFeatureProvider == null) return;

            //        manager.FeatureProviders.Remove(oldMetadataReferenceFeatureProvider);
            //        manager.FeatureProviders.Add(new ReferencesMetadataReferenceFeatureProvider());
            //    });


            //services.AddMvc().ConfigureApplicationPartManager(manager =>
            //{
            //    var oldMetadataReferenceFeatureProvider = manager.FeatureProviders.FirstOrDefault(f => f is MetadataReferenceFeatureProvider);
            //    if (oldMetadataReferenceFeatureProvider == null) return;

            //    manager.FeatureProviders.Remove(oldMetadataReferenceFeatureProvider);
            //    manager.FeatureProviders.Add(new ReferencesMetadataReferenceFeatureProvider());
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
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })

                .AddCookie(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    //options.LogoutPath = new PathString("/signout");
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

                    /*
                     * 
                     *  Add these
                     *  OpenIdConnectConstants.Scopes.OpenId,
                        OpenIdConnectConstants.Scopes.Email,
                        OpenIdConnectConstants.Scopes.Profile,
                        OpenIdConnectConstants.Scopes.OfflineAccess,
                        OpenIddictConstants.Scopes.Roles
                    *
                    */

                    options.Scope.Add(OpenIdConnectConstants.Scopes.OpenId);
                    options.Scope.Add(OpenIdConnectConstants.Scopes.Email);
                    options.Scope.Add(OpenIdConnectConstants.Scopes.Profile);
                    options.Scope.Add(OpenIdConnectConstants.Scopes.OfflineAccess);
                    options.Scope.Add(OpenIddictConstants.Scopes.Roles);
                });


            services.AddMvc();

            services.AddSingleton<HttpClient>();

            //.AddJwtBearer(options =>
            //{
            //    options.Authority = opts.Jwt.Authority;
            //    options.Audience = opts.Jwt.Audience;
            //    options.RequireHttpsMetadata = false;
            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidateIssuerSigningKey = true,
            //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(opts.Jwt.SecretKey)),
            //        ValidateIssuer = true,
            //        ValidIssuer = Core.Utilities.Configuration.ConfigurationBinder.Get<AppOptions>(Configuration).Jwt.Authority,
            //        ValidateAudience = true,
            //        ValidAudiences = new[] { opts.Jwt.Audience },
            //        ValidateLifetime = true,
            //        NameClaimType = OpenIdConnectConstants.Claims.Subject,
            //        RoleClaimType = OpenIdConnectConstants.Claims.Role
            //    };
            //});
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddSerilog();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseStatusCodePagesWithReExecute("/error");
            app.UseMvcWithDefaultRoute();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "apidocs";
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My Web API");
            });
        }
    }



}
