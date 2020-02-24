using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Web.Website.AspNetCore
{
    public static class UmbracoBackOfficeServiceCollectionExtensions
    {
        public static IServiceCollection AddUmbracoWebsite(this IServiceCollection services)
        {
            return services;
        }

    }
}
