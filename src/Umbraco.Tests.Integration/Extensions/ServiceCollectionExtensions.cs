using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Umbraco.Core.Composing;
using Umbraco.Tests.Integration.Implementations;

namespace Umbraco.Tests.Integration.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// These services need to be manually added because they do not get added by the generic host
        /// </summary>
        /// <param name="services"></param>
        /// <param name="testHelper"></param>
        /// <param name="webHostEnvironment"></param>
        public static void AddRequiredNetCoreServices(this IServiceCollection services, TestHelper testHelper, IWebHostEnvironment webHostEnvironment)
        {
            services.AddSingleton<IHttpContextAccessor>(x => testHelper.GetHttpContextAccessor());
            // the generic host does add IHostEnvironment but not this one because we are not actually in a web context
            services.AddSingleton<IWebHostEnvironment>(x => webHostEnvironment);
            // replace the IHostEnvironment that generic host created too
            services.AddSingleton<IHostEnvironment>(x => webHostEnvironment);
        }
    }
}
