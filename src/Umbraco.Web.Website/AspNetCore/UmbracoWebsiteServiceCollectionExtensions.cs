using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.DependencyInjection;

namespace Umbraco.Web.Website.AspNetCore
{
    public static class UmbracoBackOfficeServiceCollectionExtensions
    {
        public static IServiceCollection AddUmbracoWebsite(this IServiceCollection services)
        {
            services.AddImageSharp();


            return services;
        }

    }
}
