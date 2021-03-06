﻿using System;
using System.Threading;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Core;
using OpenIddict.Models;
using Serilog;
using Microsoft.AspNetCore.Hosting;
using Mvc.Server.Infrastructure.Attributes;
using System.Linq;
using Mvc.Server.Infrastructure.Utils;
using Mvc.Server.Database;
using MvcServer.Entities;
using Microsoft.AspNetCore.Identity;
using Mvc.Server.Core;
using System.Text;
using OwaspHeaders.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OwaspHeaders.Core.Extensions;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using Mvc.Server.Auth.Infra.Configuration;

namespace Mvc.Server.Auth
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

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(CustomExceptionFilterAttribute));
                options.Filters.Add(typeof(ValidateModelFilterAttribute));
            }).AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver =
                    new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            }).ConfigureApplicationPartManager(manager =>
            {
                var oldMetadataReferenceFeatureProvider = manager.FeatureProviders.FirstOrDefault(f => f is MetadataReferenceFeatureProvider);
                if (oldMetadataReferenceFeatureProvider == null) return;

                manager.FeatureProviders.Remove(oldMetadataReferenceFeatureProvider);
                manager.FeatureProviders.Add(new ReferencesMetadataReferenceFeatureProvider());
            });

            //// Add Swagger generator
            //services.AddSwaggerGen(options =>
            //{
            //    options.SwaggerDoc("v1",
            //        new Info
            //        {
            //            Title = "Api Starter Swagger",
            //            Version = "v1"
            //        });
            //});

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // Configure the context to use Microsoft SQL Server.
                options.UseSqlServer(opts.ConnectionStrings.SqlServerProvider);

                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need
                // to replace the default OpenIddict entities.
                options.UseOpenIddict();
                options.EnableSensitiveDataLogging(true);
            });

            // Register the Identity services.
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Configure Identity to use the same JWT claims as OpenIddict instead
            // of the legacy WS-Federation claims it uses by default (ClaimTypes),
            // which saves you from doing the mapping in your authorization controller.
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;

                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 1;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.User.AllowedUserNameCharacters = ApplicationConstants.AllowedUsernameCharacters;
                options.User.RequireUniqueEmail = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.Lockout.MaxFailedAccessAttempts = 3;
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

            // Register the OpenIddict services.
            services.AddOpenIddict(options =>
            {
                // Register the Entity Framework stores.
                options.AddEntityFrameworkCoreStores<ApplicationDbContext>();

                // Register the ASP.NET Core MVC binder used by OpenIddict.
                // Note: if you don't call this method, you won't be able to
                // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                options.AddMvcBinders();

                // Enable the authorization, logout, token and userinfo endpoints per configuration
                if (opts.Auth.EnableAuthorize)
                {
                    options.EnableAuthorizationEndpoint(opts.Auth.AuthorizeEndpoint);
                }

                if(opts.Auth.EnableLogout)
                {
                    options.EnableLogoutEndpoint(opts.Auth.LogoutEndpoint);
                }

                if(opts.Auth.EnableToken)
                {
                    options.EnableTokenEndpoint(opts.Auth.TokenEndpoint);
                }

                if(opts.Auth.EnableUserInfo)
                {
                    options.EnableUserinfoEndpoint(opts.Auth.UserInfoEndpoint);
                }

                if(opts.Auth.EnableIntrospection)
                {
                    options.EnableIntrospectionEndpoint(opts.Auth.IntrospectionEndpoint);
                }

                if(opts.Auth.AllowPasswordFlow)
                {
                    options.AllowPasswordFlow();
                }

                if(opts.Auth.AllowRefreshTokenFlow)
                {
                    options.AllowRefreshTokenFlow();
                }

                if(opts.Auth.AllowAuthorizationCodeFlow)
                {
                    options.AllowAuthorizationCodeFlow();
                }

                if(opts.Auth.AllowClientCredentialsFlow)
                {
                    options.AllowClientCredentialsFlow();
                }

                if(opts.Auth.AllowImplicitFlow)
                {
                    options.AllowImplicitFlow();
                }

                options.RegisterScopes(OpenIdConnectConstants.Scopes.OpenId);
                options.RegisterScopes(OpenIdConnectConstants.Scopes.Email);
                options.RegisterScopes(OpenIdConnectConstants.Scopes.Profile);
                options.RegisterScopes(OpenIdConnectConstants.Scopes.OfflineAccess);
                options.RegisterScopes(OpenIddictConstants.Scopes.Roles);

                // When request caching is enabled, authorization and logout requests
                // are stored in the distributed cache by OpenIddict and the user agent
                // is redirected to the same page with a single parameter (request_id).
                // This allows flowing large OpenID Connect requests even when using
                // an external authentication provider like Google, Facebook or Twitter.
                options.EnableRequestCaching();
                
                if(!opts.Auth.UseHttps)
                {
                    // During development, you can disable the HTTPS requirement.
                    options.DisableHttpsRequirement();
                }

                //NOTE: change this to a real certificate in prod. 
                options.AddDevelopmentSigningCertificate();
            });

            services.AddAuthentication().AddOAuthValidation();
            services.AddScoped<AuthorizationProvider>();
            services.Configure<SecureHeadersMiddlewareConfiguration>(
                Configuration.GetSection(ApplicationConstants.SecureSectionConfigurationPath));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            ILoggerFactory loggerFactory, IOptions<SecureHeadersMiddlewareConfiguration> secureHeaderSettings)
        {
            app.UseStaticFiles();
            app.UseFileServer();

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

            ////app.UseExampleMiddleware();
            //app.UseStatusCodePagesWithReExecute("/error");

            app.UseCors(options =>
            {
                options.AllowAnyHeader();
                options.AllowAnyMethod();
                options.AllowAnyOrigin();
                options.AllowCredentials();
            });

            app.UseAuthentication();

            app.UseSecureHeadersMiddleware(secureHeaderSettings.Value);
            app.UseMvcWithDefaultRoute();

            //// Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
            //app.UseSwaggerUI(c =>
            //{
            //    c.RoutePrefix = "apidocs";
            //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My Web API");
            //});

            //app.UseSwagger();
            if (env.IsDevelopment())
            {
                // Seed the database with the sample applications.
                // Note: in a real world application, this step should be part of a setup script.
                InitializeAsync(app.ApplicationServices, CancellationToken.None).GetAwaiter().GetResult();
            }
        }

        private static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
            // Create a new service scope to ensure the database context is correctly disposed when this methods returns.
            using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.Database.EnsureCreatedAsync(cancellationToken);

                var manager = scope.ServiceProvider.GetRequiredService<OpenIddictApplicationManager<OpenIddictApplication>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

                if (!roleManager.Roles.Any(x => x.Name == "Admin"))
                {
                    await roleManager.CreateAsync(new ApplicationRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Admin",
                        NormalizedName = "admin",
                    });

                    var adminRole = roleManager.Roles.SingleOrDefault(x => x.Name == "Admin");

                    if (adminRole == null)
                        throw new Exception("Newly created role Admin could not be found");
                }

                if (!roleManager.Roles.Any(x => x.Name == "User"))
                {
                    await roleManager.CreateAsync(new ApplicationRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "User",
                        NormalizedName = "user"
                    });

                    var userRole = roleManager.Roles.SingleOrDefault(x => x.Name == "User");

                    if (userRole == null)
                        throw new Exception("Newly created role Admin could not be found");
                }

                if (await userManager.FindByEmailAsync("cioclea.doru@gmail.com") == null)
                {
                    // create the user 
                    var applicationUser = new ApplicationUser
                    {
                        Email = "cioclea.doru@gmail.com",
                        EmailConfirmed = true,
                        Id = Guid.NewGuid().ToString(),
                        UserName = "doruc"
                    };

                    var result = await userManager.CreateAsync(applicationUser, "secret");
                    if (!result.Succeeded)
                    {
                        var builder = new StringBuilder();
                        foreach (var err in result.Errors)
                        {
                            builder.AppendLine(err.Description);
                        }

                        throw new Exception(builder.ToString());
                    }

                    await userManager.SetLockoutEnabledAsync(applicationUser, false);
                    await userManager.AddToRolesAsync(applicationUser, new[] { "Admin", "User" });

                }

                if (await userManager.FindByEmailAsync("cioclea.doru2@gmail.com") == null)
                {
                    // create the user 
                    var applicationUser = new ApplicationUser
                    {
                        Email = "cioclea.doru2@gmail.com",
                        EmailConfirmed = true,
                        Id = Guid.NewGuid().ToString(),
                        UserName = "doruc1"
                    };

                    var result = await userManager.CreateAsync(applicationUser, "secret");
                    if (!result.Succeeded)
                    {
                        StringBuilder builder = new StringBuilder();
                        foreach (var err in result.Errors)
                        {
                            builder.AppendLine(err.Description);
                        }

                        throw new Exception(builder.ToString());
                    }

                    await userManager.SetLockoutEnabledAsync(applicationUser, false);
                    await userManager.AddToRolesAsync(applicationUser, new[] { "User" });
                }

                if (await manager.FindByClientIdAsync("mvc", cancellationToken) == null)
                {

                    var application = new OpenIddictApplicationDescriptor
                    {
                        ClientId = "mvc",
                        DisplayName = "MVC client application",
                        RedirectUris = { new Uri("http://localhost:5000/signin-oidc") },
                        PostLogoutRedirectUris = { new Uri("http://localhost:5000/signout-callback-oidc") },
                        ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654",
                    };

                    await manager.CreateAsync(application, cancellationToken);
                }

                if (await manager.FindByClientIdAsync("api", cancellationToken) == null)
                {

                    var application = new OpenIddictApplicationDescriptor
                    {
                        ClientId = "api",
                        DisplayName = "API client application",
                        ClientSecret = "024f8e6c-d72f-4dad-975f-3bfbfc922427",
                    };

                    await manager.CreateAsync(application, cancellationToken);
                }

                // To test this sample with Postman, use the following settings:
                //
                // * Authorization URL: http://localhost:5001/connect/authorize
                // * Access token URL: http://localhost:5001/connect/token
                // * Client ID: postman
                // * Client secret: [blank] (not used with public clients)
                // * Scope: openid email profile roles
                // * Grant type: authorization code
                // * Request access token locally: yes
                if (await manager.FindByClientIdAsync("postman", cancellationToken) == null)
                {
                    var application = new OpenIddictApplicationDescriptor
                    {
                        ClientId = "postman",
                        DisplayName = "Postman",
                        RedirectUris = { new Uri("https://www.getpostman.com/oauth2/callback") }
                    };

                    await manager.CreateAsync(application, cancellationToken);
                }
            }
        }
    }
}
