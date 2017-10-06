using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using AspNet.Security.OAuth.Introspection;
using AspNet.Security.OAuth.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Mvc.Server.Database;
using Mvc.Server.DataObjects.Configuration;
using Mvc.Server.Infrastructure.Filters;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using Mvc.Server.Infrastructure.Attributes;
using Mvc.Server.Infrastructure.Security;
using Mvc.Server.Infrastructure.Utils;

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

            services.AddAuthentication(OAuthValidationDefaults.AuthenticationScheme);

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
            });

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();


            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = OAuthIntrospectionDefaults.AuthenticationScheme;
                })

                .AddOAuthIntrospection(options =>
                {
                    options.Authority = new Uri("http://localhost:5001/");
                    options.Audiences.Add("mvc");
                    options.ClientId = "mvc";
                    options.ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654";
                    options.RequireHttpsMetadata = false;

                    // Note: you can override the default name and role claims:
                    // options.NameClaimType = "custom_name_claim";
                    // options.RoleClaimType = "custom_role_claim";
                });

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })

                .AddJwtBearer(options =>
                {
                    options.Authority = opts.Jwt.Authority;
                    options.Audience = opts.Jwt.Audience;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(opts.Jwt.SecretKey)),
                        ValidateIssuer = true,
                        ValidIssuer = Core.Utilities.Configuration.ConfigurationBinder.Get<AppOptions>(Configuration).Jwt.Authority,
                        ValidateAudience = true,
                        ValidAudiences = new[] { opts.Jwt.Audience },
                        ValidateLifetime = true,
                    };
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddSerilog();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseStatusCodePagesWithReExecute("/error");
            app.UseAuthentication();
            //app.UseExampleMiddleware();
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
