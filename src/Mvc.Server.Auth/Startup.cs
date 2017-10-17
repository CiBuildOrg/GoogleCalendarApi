using System;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Mvc.Server.Core;
using Mvc.Server.Database;
using Mvc.Server.DataObjects.Configuration;
using Mvc.Server.Infrastructure.Attributes;
using Mvc.Server.Infrastructure.Security;
using Mvc.Server.Infrastructure.Utils;
using MvcServer.Entities;
using OpenIddict.Core;
using OpenIddict.Models;
using OwaspHeaders.Core.Extensions;
using OwaspHeaders.Core.Models;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using Mvc.Server.Infrastructure.Filters;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using AspNet.Security.OpenIdConnect.Server;

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

            // Add Swagger generator
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1",
                    new Info
                    {
                        Title = "Api Starter Swagger",
                        Version = "v1"
                    });
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

            //Add MVC Core
            services.AddMvcCore(
                    options =>
                    {
                        // Add global authorization filter 
                        var policy = new AuthorizationPolicyBuilder()
                            .RequireAuthenticatedUser()
                            .Build();
                        options.Filters.Add(new ApplicationAuthorizeFilter(policy));
                        // Add global exception handler for production
                        options.Filters.Add(typeof(CustomExceptionFilterAttribute));
                        // Add global validation filter
                        options.Filters.Add(typeof(ValidateModelFilterAttribute));

                    }
                )
                .AddJsonFormatters()
                .AddAuthorization(options =>
                {
                    // Create a policy for each permission
                    foreach (var permissionClaim in PermissionClaims.GetAll())
                    {
                        options.AddPolicy(permissionClaim, policy => policy.Requirements.Add(new PermissionRequirement(permissionClaim)));
                    }
                })
                .AddDataAnnotations()
                .AddCors()
                .AddApiExplorer().ConfigureApplicationPartManager(manager =>
                {
                    var oldMetadataReferenceFeatureProvider = manager.FeatureProviders.FirstOrDefault(f => f is MetadataReferenceFeatureProvider);
                    if (oldMetadataReferenceFeatureProvider == null) return;

                    manager.FeatureProviders.Remove(oldMetadataReferenceFeatureProvider);
                    manager.FeatureProviders.Add(new ReferencesMetadataReferenceFeatureProvider());
                });

            services.AddMvc().ConfigureApplicationPartManager(manager =>
            {
                var oldMetadataReferenceFeatureProvider = manager.FeatureProviders.FirstOrDefault(f => f is MetadataReferenceFeatureProvider);
                if (oldMetadataReferenceFeatureProvider == null) return;

                manager.FeatureProviders.Remove(oldMetadataReferenceFeatureProvider);
                manager.FeatureProviders.Add(new ReferencesMetadataReferenceFeatureProvider());
            });

            services.AddScoped<IAuthorizationHandler, PermissionHandler>();
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // Configure the context to use Microsoft SQL Server.
                options.UseSqlServer(opts.ConnectionStrings.SqlServerProvider);

                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need
                // to replace the default OpenIddict entities.
                options.UseOpenIddict();
            });


            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "ServerCookie";
            })

            .AddCookie("ServerCookie", options =>
            {
                options.Cookie.Name = CookieAuthenticationDefaults.CookiePrefix + "ServerCookie";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
                options.LoginPath = new PathString("/signin");
                options.LogoutPath = new PathString("/signout");
            }).AddOAuthValidation();

            // Register the OpenIddict services.
            services.AddOpenIddict(options =>
            {

                // Register the Entity Framework stores.
                options.AddEntityFrameworkCoreStores<ApplicationDbContext>();

                // Register the ASP.NET Core MVC binder used by OpenIddict.
                // Note: if you don't call this method, you won't be able to
                // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                options.AddMvcBinders();

                // Enable the authorization, logout, token and userinfo endpoints.
                options.EnableAuthorizationEndpoint(opts.Auth.AuthorizeEndpoint)
                    .EnableLogoutEndpoint(opts.Auth.LogoutEndpoint)
                    .EnableTokenEndpoint(opts.Auth.TokenEndpoint)
                    .EnableUserinfoEndpoint(opts.Auth.UserInfoEndpoint);

                // Note: the Mvc.Client sample only uses the authorization code flow but you can enable
                // the other flows if you need to support implicit, password or client credentials.
                //options.AllowAuthorizationCodeFlow();

                options
                    .AllowPasswordFlow()
                    .AllowRefreshTokenFlow()
                    .AllowAuthorizationCodeFlow()
                    .AllowClientCredentialsFlow();

                // When request caching is enabled, authorization and logout requests
                // are stored in the distributed cache by OpenIddict and the user agent
                // is redirected to the same page with a single parameter (request_id).
                // This allows flowing large OpenID Connect requests even when using
                // an external authentication provider like Google, Facebook or Twitter.
                options.EnableRequestCaching();
                // During development, you can disable the HTTPS requirement.
                options.DisableHttpsRequirement();

                // Note: to use JWT access tokens instead of the default
                // encrypted format, the following lines are required:
                //
                //options.UseJsonWebTokens();
                //options.AddEphemeralSigningKey();

                options.AddSigningKey(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
                    Core.Utilities.Configuration.ConfigurationBinder.Get<AppOptions>(Configuration).Jwt.SecretKey)));

            });

            //services.AddAuthentication(Oauth.AuthenticationScheme)


            //JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            //JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //    .AddJwtBearer(options =>
            //    {
            //        options.Authority = opts.Jwt.Authority;
            //        options.Audience = opts.Jwt.Audience;
            //        options.RequireHttpsMetadata = false;
            //        options.TokenValidationParameters = new TokenValidationParameters
            //        {
            //            ValidateIssuerSigningKey = true,
            //            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(opts.Jwt.SecretKey)),
            //            ValidateIssuer = true,
            //            ValidIssuer = Core.Utilities.Configuration.ConfigurationBinder.Get<AppOptions>(Configuration)
            //                .Jwt.Authority,
            //            ValidateAudience = true,
            //            ValidAudiences = new[] { opts.Jwt.Audience },
            //            ValidateLifetime = true,
            //        };
            //    });

            services.AddScoped<AuthorizationProvider>();

            services.Configure<SecureHeadersMiddlewareConfiguration>(
                Configuration.GetSection("SecureHeadersMiddlewareConfiguration"));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            ILoggerFactory loggerFactory, IOptions<SecureHeadersMiddlewareConfiguration> secureHeaderSettings)
        {
            loggerFactory.AddSerilog();
            app.UseStaticFiles();

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
            app.UseStatusCodePagesWithReExecute("/error");

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

            // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "apidocs";
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My Web API");
            });

            app.UseSwagger();
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

                    foreach (var claim in PermissionClaims.GetAdminClaims())
                    {
                        await roleManager.AddClaimAsync(adminRole, new Claim(ApplicationConstants.PermissionClaimName, claim));
                    }
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

                    foreach (var claim in PermissionClaims.GetAppUserClaims())
                    {
                        await roleManager.AddClaimAsync(userRole, new Claim(ApplicationConstants.PermissionClaimName, claim));
                    }
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

    public sealed class AuthorizationProvider : OpenIdConnectServerProvider
    {
        private readonly ApplicationDbContext _database;

        public AuthorizationProvider(ApplicationDbContext database)
        {
            _database = database;
        }

        public override async Task ValidateAuthorizationRequest(ValidateAuthorizationRequestContext context)
        {
            // Note: the OpenID Connect server middleware supports the authorization code, implicit and hybrid flows
            // but this authorization provider only accepts response_type=code authorization/authentication requests.
            // You may consider relaxing it to support the implicit or hybrid flows. In this case, consider adding
            // checks rejecting implicit/hybrid authorization requests when the client is a confidential application.
            if (!context.Request.IsAuthorizationCodeFlow())
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.UnsupportedResponseType,
                    description: "Only the authorization code flow is supported by this authorization server.");

                return;
            }

            // Note: to support custom response modes, the OpenID Connect server middleware doesn't
            // reject unknown modes before the ApplyAuthorizationResponse event is invoked.
            // To ensure invalid modes are rejected early enough, a check is made here.
            if (!string.IsNullOrEmpty(context.Request.ResponseMode) && !context.Request.IsFormPostResponseMode() &&
                                                                       !context.Request.IsFragmentResponseMode() &&
                                                                       !context.Request.IsQueryResponseMode())
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidRequest,
                    description: "The specified 'response_mode' is unsupported.");

                return;
            }

            // Retrieve the application details corresponding to the requested client_id.
            var application = await (from entity in _database.OpenIdApplications
                                     where entity.ClientId == context.ClientId
                                     select entity).SingleOrDefaultAsync(context.HttpContext.RequestAborted);

            if (application == null)
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidClient,
                    description: "The specified client identifier is invalid.");

                return;
            }

            if (!string.IsNullOrEmpty(context.RedirectUri) &&
                !string.Equals(context.RedirectUri, application.RedirectUris, StringComparison.Ordinal))
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidClient,
                    description: "The specified 'redirect_uri' is invalid.");

                return;
            }

            context.Validate(application.RedirectUris);
        }

        public override async Task ValidateTokenRequest(ValidateTokenRequestContext context)
        {
            // Note: the OpenID Connect server middleware supports authorization code, refresh token, client credentials
            // and resource owner password credentials grant types but this authorization provider uses a safer policy
            // rejecting the last two ones. You may consider relaxing it to support the ROPC or client credentials grant types.
            if (!context.Request.IsAuthorizationCodeGrantType() && !context.Request.IsRefreshTokenGrantType())
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.UnsupportedGrantType,
                    description: "Only authorization code and refresh token grant types " +
                                 "are accepted by this authorization server.");

                return;
            }

            // Note: client authentication is not mandatory for non-confidential client applications like mobile apps
            // (except when using the client credentials grant type) but this authorization server uses a safer policy
            // that makes client authentication mandatory and returns an error if client_id or client_secret is missing.
            // You may consider relaxing it to support the resource owner password credentials grant type
            // with JavaScript or desktop applications, where client credentials cannot be safely stored.
            // In this case, call context.Skip() to inform the server middleware the client is not trusted.
            if (string.IsNullOrEmpty(context.ClientId) || string.IsNullOrEmpty(context.ClientSecret))
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidRequest,
                    description: "The mandatory 'client_id'/'client_secret' parameters are missing.");

                return;
            }

            // Retrieve the application details corresponding to the requested client_id.
            var application = await (from entity in _database.OpenIdApplications
                                     where entity.ClientId == context.ClientId
                                     select entity).SingleOrDefaultAsync(context.HttpContext.RequestAborted);

            if (application == null)
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidClient,
                    description: "The specified client identifier is invalid.");

                return;
            }

            // Note: to mitigate brute force attacks, you SHOULD strongly consider applying
            // a key derivation function like PBKDF2 to slow down the secret validation process.
            // You SHOULD also consider using a time-constant comparer to prevent timing attacks.
            // For that, you can use the CryptoHelper library developed by @henkmollema:
            // https://github.com/henkmollema/CryptoHelper. If you don't need .NET Core support,
            // SecurityDriven.NET/inferno is a rock-solid alternative: http://securitydriven.net/inferno/
            if (!string.Equals(context.ClientSecret, application.ClientSecret, StringComparison.Ordinal))
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidClient,
                    description: "The specified client credentials are invalid.");

                return;
            }

            context.Validate();
        }

        public override async Task ValidateLogoutRequest(ValidateLogoutRequestContext context)
        {
            // When provided, post_logout_redirect_uri must exactly
            // match the address registered by the client application.
            if (!string.IsNullOrEmpty(context.PostLogoutRedirectUri) &&
                !await _database.OpenIdApplications.AnyAsync(application => application.RedirectUris == context.PostLogoutRedirectUri))
            {
                context.Reject(
                    error: OpenIdConnectConstants.Errors.InvalidRequest,
                    description: "The specified 'post_logout_redirect_uri' is invalid.");

                return;
            }

            context.Validate();
        }
    }
}
