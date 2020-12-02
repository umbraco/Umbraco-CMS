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
            // TODO: Should this only exist in the back office project? These really are only ever used for the back office AFAIK
            // If it is moved it should only target the back office scheme

            services.AddSingleton<IAuthorizationHandler, FeatureAuthorizeHandler>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthorizationPolicies.UmbracoFeatureEnabled, policy =>
                    policy.Requirements.Add(new FeatureAuthorizeRequirement()));
            });
        }
    }

}
