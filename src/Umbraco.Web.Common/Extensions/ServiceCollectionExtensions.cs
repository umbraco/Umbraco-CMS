using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Web.BackOffice.Authorization;
using Umbraco.Web.Common.Authorization;

namespace Umbraco.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddUmbracoCommonAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationHandler, FeatureAuthorizeHandler>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthorizationPolicies.UmbracoFeatureEnabled, policy =>
                    policy.Requirements.Add(new FeatureAuthorizeRequirement()));
            });
        }
    }

}
