using Microsoft.Extensions.DependencyInjection;
using Umbraco.Web.Common.Middleware;

namespace Umbraco.Web.Common.Extensions
{
    public static class UmbracoRequestServiceCollectionExtensions
    {
        public static IServiceCollection AddUmbracoRequest(this IServiceCollection services)
        {
            var umbracoRequestLifetime = new UmbracoRequestLifetime();

            services.AddSingleton<IUmbracoRequestLifetimeManager>(umbracoRequestLifetime);
            services.AddSingleton<IUmbracoRequestLifetime>(umbracoRequestLifetime);

            return services;
        }

    }

}
