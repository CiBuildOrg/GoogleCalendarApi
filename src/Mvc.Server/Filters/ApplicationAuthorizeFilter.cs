using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Mvc.Server.Filters
{
    /// <summary>
    /// An implementation of <see cref="T:Microsoft.AspNetCore.Mvc.Filters.IAsyncAuthorizationFilter" /> which applies a specific
    /// <see cref="T:Microsoft.AspNetCore.Authorization.AuthorizationPolicy" />. MVC recognizes the <see cref="T:Microsoft.AspNetCore.Authorization.AuthorizeAttribute" /> and adds an instance of
    /// this filter to the associated action or controller.
    /// </summary>
    public class ApplicationAuthorizeFilter : IAsyncAuthorizationFilter, IFilterFactory
    {
        /// <summary>
        /// The <see cref="T:Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider" /> to use to resolve policy names.
        /// </summary>
        public IAuthorizationPolicyProvider PolicyProvider { get; }

        /// <summary>
        /// The <see cref="T:Microsoft.AspNetCore.Authorization.IAuthorizeData" /> to combine into an <see cref="T:Microsoft.AspNetCore.Authorization.IAuthorizeData" />.
        /// </summary>
        public IEnumerable<IAuthorizeData> AuthorizeData { get; }

        /// <summary>Gets the authorization policy to be used.</summary>
        /// <remarks>
        /// If<c>null</c>, the policy will be constructed using
        /// <see cref="M:Microsoft.AspNetCore.Authorization.AuthorizationPolicy.CombineAsync(Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider,System.Collections.Generic.IEnumerable{Microsoft.AspNetCore.Authorization.IAuthorizeData})" />.
        /// </remarks>
        public AuthorizationPolicy Policy { get; }

        bool IFilterFactory.IsReusable => true;

        /// <summary>
        /// Initialize a new <see cref="T:Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter" /> instance.
        /// </summary>
        /// <param name="policy">Authorization policy to be used.</param>
        public ApplicationAuthorizeFilter(AuthorizationPolicy policy)
        {
            Policy = policy ?? throw new ArgumentNullException(nameof(policy));
        }

        /// <summary>
        /// Initialize a new <see cref="T:Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter" /> instance.
        /// </summary>
        /// <param name="policyProvider">The <see cref="T:Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider" /> to use to resolve policy names.</param>
        /// <param name="authorizeData">The <see cref="T:Microsoft.AspNetCore.Authorization.IAuthorizeData" /> to combine into an <see cref="T:Microsoft.AspNetCore.Authorization.IAuthorizeData" />.</param>
        public ApplicationAuthorizeFilter(IAuthorizationPolicyProvider policyProvider, IEnumerable<IAuthorizeData> authorizeData)
            : this(authorizeData)
        {
            PolicyProvider = policyProvider ?? throw new ArgumentNullException(nameof(policyProvider));
        }

        /// <summary>
        /// Initializes a new instance of <see cref="T:Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter" />.
        /// </summary>
        /// <param name="authorizeData">The <see cref="T:Microsoft.AspNetCore.Authorization.IAuthorizeData" /> to combine into an <see cref="T:Microsoft.AspNetCore.Authorization.IAuthorizeData" />.</param>
        public ApplicationAuthorizeFilter(IEnumerable<IAuthorizeData> authorizeData)
        {
            AuthorizeData = authorizeData ?? throw new ArgumentNullException(nameof(authorizeData));
        }

        /// <summary>
        /// Initializes a new instance of <see cref="T:Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter" />.
        /// </summary>
        /// <param name="policy">The name of the policy to require for authorization.</param>
        public ApplicationAuthorizeFilter(string policy)
            : this((IEnumerable<IAuthorizeData>) new[]
            {
                new AuthorizeAttribute(policy)
            })
        {
        }

        /// <inheritdoc />
        public virtual async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var filters = context.Filters;

            if (context.ActionDescriptor is ControllerActionDescriptor descriptor)
            {
                var attributes = descriptor.MethodInfo.GetCustomAttributes(true);
                var routeAttribute = attributes.SingleOrDefault(x => x.GetType() == typeof(RouteAttribute));

                if (routeAttribute != null)
                {
                    var attribute = (RouteAttribute)routeAttribute;

                    if (attribute.Template == "~/error")
                        return;
                }

                // if we ever have allowAnonymous on methods 
                if (attributes.Any(x => x.GetType() == typeof(AllowAnonymousAttribute)))
                {
                    return;
                }

                var authorizeAttribute = attributes.SingleOrDefault(x => x.GetType() == typeof(AuthorizeAttribute));
                if (authorizeAttribute != null)
                {
                    var attribute = (AuthorizeAttribute)authorizeAttribute;
                    if (attribute.AuthenticationSchemes == OAuthValidationDefaults.AuthenticationScheme)
                    {
                        return;
                    }
                }
            }

            bool Func(IFilterMetadata item) => item is IAllowAnonymousFilter;
            if (filters.Any(Func))
                return;

            if (context == null)
                throw new ArgumentNullException(nameof(context));
            var effectivePolicy = Policy;
            if (effectivePolicy == null)
            {
                if (PolicyProvider == null)
                    throw new InvalidOperationException("Auth policy cannot be created");
                effectivePolicy = await AuthorizationPolicy.CombineAsync(PolicyProvider, AuthorizeData);
            }
            if (effectivePolicy == null)
                return;
            var policyEvaluator = context.HttpContext.RequestServices.GetRequiredService<IPolicyEvaluator>();
            var authenticationResult = await policyEvaluator.AuthenticateAsync(effectivePolicy, context.HttpContext);

            

            var authorizationResult = await policyEvaluator.AuthorizeAsync(effectivePolicy, authenticationResult, context.HttpContext, context);
            if (authorizationResult.Challenged)
            {
                context.Result = new ChallengeResult(effectivePolicy.AuthenticationSchemes.ToArray());
            }
            else
            {
                if (!authorizationResult.Forbidden)
                    return;
                context.Result = new ForbidResult(effectivePolicy.AuthenticationSchemes.ToArray());
            }
        }

        IFilterMetadata IFilterFactory.CreateInstance(IServiceProvider serviceProvider)
        {
            if (Policy != null || PolicyProvider != null)
                return this;
            return AuthorizationApplicationModelProvider.GetFilter(serviceProvider.GetRequiredService<IAuthorizationPolicyProvider>(), AuthorizeData);
        }
    }
}