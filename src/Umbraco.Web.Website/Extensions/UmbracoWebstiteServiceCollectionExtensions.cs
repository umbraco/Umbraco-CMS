using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Extensions
{
    public static class UmbracoWebstiteServiceCollectionExtensions
    {
        public static void AddUmbracoWebsite(this IServiceCollection services)
        {
            services.AddSingleton<IControllerActivator, ServiceBasedControllerActivator>();
        }
    }
}
