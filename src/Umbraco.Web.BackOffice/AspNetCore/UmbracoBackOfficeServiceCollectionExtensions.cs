using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Umbraco.Composing;
using Umbraco.Configuration;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Logging.Serilog;

namespace Umbraco.Web.BackOffice.AspNetCore
{
    public static class UmbracoBackOfficeServiceCollectionExtensions
    {
        public static IServiceCollection AddUmbracoBackOffice(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            var serviceProvider = services.BuildServiceProvider();

            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            var webHostEnvironment = serviceProvider.GetService<IWebHostEnvironment>();
            var hostApplicationLifetime = serviceProvider.GetService<IHostApplicationLifetime>();
            var configuration = serviceProvider.GetService<IConfiguration>();

            var configsFactory = new AspNetCoreConfigsFactory(configuration);
            var configs = configsFactory.Create();

            var settings = configs.GetConfig<IModelsBuilderConfig>();

            var x =settings.ModelsDirectory;


            services.CreateCompositionRoot(
                httpContextAccessor,
                webHostEnvironment,
                hostApplicationLifetime,
                configsFactory);

            return services;
        }


        public static IServiceCollection CreateCompositionRoot(
            this IServiceCollection services,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment webHostEnvironment,
            IHostApplicationLifetime hostApplicationLifetime,
            IConfigsFactory configsFactory)
        {
            var configFactory = new ConfigsFactory();
            var hostingSettings = configFactory.HostingSettings;
            var coreDebug = configFactory.CoreDebugSettings;
            var globalSettings = configFactory.GlobalSettings;

            var hostingEnvironment = new AspNetCoreHostingEnvironment(hostingSettings, webHostEnvironment, httpContextAccessor, hostApplicationLifetime);
            var ioHelper = new IOHelper(hostingEnvironment, globalSettings);
            var logger = SerilogLogger.CreateWithDefaultConfiguration(hostingEnvironment,  new AspNetCoreSessionIdResolver(httpContextAccessor), () => services.BuildServiceProvider().GetService<IRequestCache>(), coreDebug, ioHelper, new AspNetCoreMarchal());
            var configs = configFactory.Create();

            var backOfficeInfo = new AspNetCoreBackOfficeInfo(globalSettings);
            var profiler = new LogProfiler(logger);

            Current.Initialize(logger, configs, ioHelper, hostingEnvironment, backOfficeInfo, profiler);

            return services;
        }
    }
}
