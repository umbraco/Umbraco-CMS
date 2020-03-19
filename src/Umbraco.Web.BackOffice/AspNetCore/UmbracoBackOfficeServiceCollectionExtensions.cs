using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Umbraco.Composing;
using Umbraco.Configuration;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Logging.Serilog;

namespace Umbraco.Web.BackOffice.AspNetCore
{
    public static class UmbracoBackOfficeServiceCollectionExtensions
    {
        public static IServiceCollection AddUmbracoConfiguration(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();
            var configsFactory = new AspNetCoreConfigsFactory(configuration);

            var configs = configsFactory.Create();

            var x = configs.GetConfig<IRequestHandlerSettings>();

            var y = x.CharCollection;
            services.AddSingleton(configs);

            return services;
        }

        public static IServiceCollection AddUmbracoBackOffice(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            var serviceProvider = services.BuildServiceProvider();

            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            var webHostEnvironment = serviceProvider.GetService<IWebHostEnvironment>();
            var hostApplicationLifetime = serviceProvider.GetService<IHostApplicationLifetime>();

            var configs = serviceProvider.GetService<Configs>();

            services.CreateCompositionRoot(
                httpContextAccessor,
                webHostEnvironment,
                hostApplicationLifetime,
                configs);

            return services;
        }


        public static IServiceCollection CreateCompositionRoot(
            this IServiceCollection services,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment webHostEnvironment,
            IHostApplicationLifetime hostApplicationLifetime,
            Configs configs)
        {
            var hostingSettings = configs.Hosting();
            var coreDebug = configs.CoreDebug();
            var globalSettings = configs.Global();

            var hostingEnvironment = new AspNetCoreHostingEnvironment(hostingSettings, webHostEnvironment,
                httpContextAccessor, hostApplicationLifetime);
            var ioHelper = new IOHelper(hostingEnvironment, globalSettings);
            var logger = SerilogLogger.CreateWithDefaultConfiguration(hostingEnvironment,
                new AspNetCoreSessionIdResolver(httpContextAccessor),
                () => services.BuildServiceProvider().GetService<IRequestCache>(), coreDebug, ioHelper,
                new AspNetCoreMarchal());

            var backOfficeInfo = new AspNetCoreBackOfficeInfo(globalSettings);
            var profiler = new LogProfiler(logger);

            Current.Initialize(logger, configs, ioHelper, hostingEnvironment, backOfficeInfo, profiler);

            return services;
        }
    }
}
