using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Mvc.Server.DataObjects.Configuration;
using Serilog;
using OpenIddict.Core;
using OwaspHeaders.Core.Extensions;
using OwaspHeaders.Core.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;
using AspNet.Security.OAuth.Introspection;
using Mvc.Server.Core;

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

            //// Add Swagger generator
            //services.AddSwaggerGen(options =>
            //{
            //    options.SwaggerDoc("v1", new Info { Title = "Api Starter", Version = "v1" });
            //});

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

            if (opts.AuthenticationSettings.UseApi)
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultScheme = OAuthIntrospectionDefaults.AuthenticationScheme;
                }).AddOAuthIntrospection(options =>
                {
                    options.Authority = new Uri(opts.AuthenticationSettings.AuthorityUrl);
                    options.Audiences.Add(opts.AuthenticationSettings.ApiSettings.Audience);
                    options.ClientId = opts.AuthenticationSettings.ApiSettings.ClientId;
                    options.ClientSecret = opts.AuthenticationSettings.ApiSettings.ClientSecret;
                    options.RequireHttpsMetadata = opts.AuthenticationSettings.ApiSettings.RequireHttps;
                    // Note: you can override the default name and role claims:
                    options.NameClaimType = OpenIdConnectConstants.Claims.Name;
                    options.RoleClaimType = OpenIdConnectConstants.Claims.Role;
                });
            }

            if (opts.AuthenticationSettings.UseWeb)
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultForbidScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.LoginPath = new PathString(ApplicationConstants.CookieLoginPath);
                    options.LogoutPath = new PathString(ApplicationConstants.CookieLogoutPath);
                }).AddOpenIdConnect(options =>
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    // Note: these settings must match the application details
                    // inserted in the database at the server level.
                    options.ClientId = opts.AuthenticationSettings.WebSettings.ClientId;
                    options.ClientSecret = opts.AuthenticationSettings.WebSettings.ClientSecret;
                    options.RequireHttpsMetadata = false;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;

                    // Use the authorization code flow.
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;
                    // Note: setting the Authority allows the OIDC client middleware to automatically
                    // retrieve the identity provider's configuration and spare you from setting
                    // the different endpoints URIs or the token validation parameters explicitly.
                    options.Authority = opts.AuthenticationSettings.AuthorityUrl;
                    options.Scope.Add(OpenIdConnectConstants.Scopes.OpenId);
                    options.Scope.Add(OpenIdConnectConstants.Scopes.Email);
                    options.Scope.Add(OpenIdConnectConstants.Scopes.Profile);
                    options.Scope.Add(OpenIdConnectConstants.Scopes.OfflineAccess);
                    options.Scope.Add(OpenIddictConstants.Scopes.Roles);
                    options.Resource = opts.AuthenticationSettings.WebSettings.Resource;

                    options.SecurityTokenValidator = new JwtSecurityTokenHandler
                    {
                        // Disable the built-in JWT claims mapping feature.
                        InboundClaimTypeMap = new Dictionary<string, string>()
                    };

                    options.TokenValidationParameters.NameClaimType = OpenIdConnectConstants.Claims.Name;
                    options.TokenValidationParameters.RoleClaimType = OpenIdConnectConstants.Claims.Role;
                    options.TokenValidationParameters.AuthenticationType = CookieAuthenticationDefaults.AuthenticationScheme;
                });
            }

            services.AddMvc();
            services.AddSingleton<HttpClient>();
            services.Configure<SecureHeadersMiddlewareConfiguration>(
               Configuration.GetSection(ApplicationConstants.SecureSectionConfigurationPath));
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
                app.UseExceptionHandler("/home/error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseSecureHeadersMiddleware(secureHeaderSettings.Value);
            app.UseMvc();

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
