using System.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Umbraco.Composing;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Logging.Serilog;
using Umbraco.Core.Runtime;

namespace Umbraco.Web.BackOffice.AspNetCore
{
    public static class UmbracoBackOfficeServiceCollectionExtensions
    {
        public static IServiceCollection AddUmbracoBackOffice(this IServiceCollection services)
        {


            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            CreateCompositionRoot(services);

            return services;
        }


        private static void CreateCompositionRoot(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();

            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            var webHostEnvironment = serviceProvider.GetService<IWebHostEnvironment>();
            var hostApplicationLifetime = serviceProvider.GetService<IHostApplicationLifetime>();

            var configFactory = new ConfigsFactory();

            var hostingSettings = configFactory.HostingSettings;
            var coreDebug = configFactory.CoreDebug;
            var globalSettings = configFactory.GlobalSettings;

            var hostingEnvironment = new AspNetCoreHostingEnvironment(hostingSettings, webHostEnvironment, httpContextAccessor, hostApplicationLifetime);
            var ioHelper = new IOHelper(hostingEnvironment, globalSettings);
            var logger = SerilogLogger.CreateWithDefaultConfiguration(hostingEnvironment,  new AspNetCoreSessionIdResolver(httpContextAccessor), () => services.BuildServiceProvider().GetService<IRequestCache>(), coreDebug, ioHelper, new AspNetCoreMarchal());
            var configs = configFactory.Create();

            var backOfficeInfo = new AspNetCoreBackOfficeInfo(globalSettings);
            var profiler = new LogProfiler(logger);

            Current.Initialize(logger, configs, ioHelper, hostingEnvironment, backOfficeInfo, profiler);
        }
    }
}
