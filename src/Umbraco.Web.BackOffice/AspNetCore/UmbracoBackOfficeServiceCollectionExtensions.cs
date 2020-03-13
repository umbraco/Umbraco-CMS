using System;
using System.Data.Common;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Umbraco.Composing;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Logging.Serilog;
using Umbraco.Core.Persistence;
using Umbraco.Core.Runtime;

namespace Umbraco.Web.BackOffice.AspNetCore
{

    public static class UmbracoBackOfficeServiceCollectionExtensions
    {

        /// <summary>
        ///  Adds the Umbraco Back Core requirements
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        /// <remarks>
        /// Must be called after all services are added to the application because we are cross-wiring the container (currently)
        /// </remarks>
        public static IServiceCollection AddUmbracoCore(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            CreateCompositionRoot(services);

            if (!UmbracoServiceProviderFactory.IsActive)
                throw new InvalidOperationException("Ensure to add UseUmbraco() in your Program.cs after ConfigureWebHostDefaults to enable Umbraco's service provider factory");

            var umbContainer = UmbracoServiceProviderFactory.UmbracoContainer;

            // TODO: Get rid of this 'Current' requirement
            var globalSettings = Current.Configs.Global();
            var umbracoVersion = new UmbracoVersion(globalSettings);

            var coreRuntime = GetCoreRuntime(
                Current.Configs,
                umbracoVersion,
                Current.IOHelper,
                Current.Logger,
                Current.Profiler,
                Current.HostingEnvironment,
                Current.BackOfficeInfo);

            var factory = coreRuntime.Boot(umbContainer);

            return services;
        }

        private static IRuntime GetCoreRuntime(Configs configs, IUmbracoVersion umbracoVersion, IIOHelper ioHelper, ILogger logger,
            IProfiler profiler, Core.Hosting.IHostingEnvironment hostingEnvironment, IBackOfficeInfo backOfficeInfo)
        {
            var connectionStringConfig = configs.ConnectionStrings()[Constants.System.UmbracoConnectionName];
            var dbProviderFactoryCreator = new SqlServerDbProviderFactoryCreator(
                connectionStringConfig?.ProviderName,
                DbProviderFactories.GetFactory);

            // Determine if we should use the sql main dom or the default
            var appSettingMainDomLock = configs.Global().MainDomLock;
            var mainDomLock = appSettingMainDomLock == "SqlMainDomLock"
                ? (IMainDomLock)new SqlMainDomLock(logger, configs, dbProviderFactoryCreator)
                : new MainDomSemaphoreLock(logger, hostingEnvironment);

            var mainDom = new MainDom(logger, hostingEnvironment, mainDomLock);

            // TODO: Currently we are not passing in any TypeFinderConfig (with ITypeFinderSettings) which we should do, however
            // this is not critical right now and would require loading in some config before boot time so just leaving this as-is for now.
            var typeFinder = new TypeFinder(logger, new DefaultUmbracoAssemblyProvider(Assembly.GetEntryAssembly()));

            var coreRuntime = new CoreRuntime(configs, umbracoVersion, ioHelper, logger, profiler, new AspNetCoreBootPermissionsChecker(),
                hostingEnvironment, backOfficeInfo, dbProviderFactoryCreator, mainDom, typeFinder);

            return coreRuntime;
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

            var hostingEnvironment = new AspNetCoreHostingEnvironment(hostingSettings, webHostEnvironment, httpContextAccessor, hostApplicationLifetime);
            var ioHelper = new IOHelper(hostingEnvironment);

            var logger = SerilogLogger.CreateWithDefaultConfiguration(hostingEnvironment, new AspNetCoreSessionIdResolver(httpContextAccessor), () => services.BuildServiceProvider().GetService<IRequestCache>(), coreDebug, ioHelper, new AspNetCoreMarchal());
            var configs = configFactory.Create(ioHelper, logger);

            var backOfficeInfo = new AspNetCoreBackOfficeInfo(configs.Global());
            var profiler = new LogProfiler(logger);

            Current.Initialize(logger, configs, ioHelper, hostingEnvironment, backOfficeInfo, profiler);
        }

        private class AspNetCoreBootPermissionsChecker : IUmbracoBootPermissionChecker
        {
            public void ThrowIfNotPermissions()
            {
                // nothing to check
            }
        }
    }
}
