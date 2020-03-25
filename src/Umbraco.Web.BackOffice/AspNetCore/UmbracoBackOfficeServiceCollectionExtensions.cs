

using System;
using System.Data.Common;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Smidge;
using Umbraco.Composing;
using Umbraco.Configuration;
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

        public static IServiceCollection AddUmbracoConfiguration(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();
            if (configuration == null)
                throw new InvalidOperationException($"Could not resolve {typeof(IConfiguration)} from the container");

            var configsFactory = new AspNetCoreConfigsFactory(configuration);

            var configs = configsFactory.Create();

            services.AddSingleton(configs);

            return services;
        }


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
            if (!UmbracoServiceProviderFactory.IsActive)
                throw new InvalidOperationException("Ensure to add UseUmbraco() in your Program.cs after ConfigureWebHostDefaults to enable Umbraco's service provider factory");

            var umbContainer = UmbracoServiceProviderFactory.UmbracoContainer;

            services.AddUmbracoCore(umbContainer, Assembly.GetEntryAssembly());

            return services;
        }

        public static IServiceCollection AddUmbracoCore(this IServiceCollection services, IRegister umbContainer, Assembly entryAssembly)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // TODO: Get rid of this 'Current' requirement
            var globalSettings = Current.Configs.Global();
            var umbracoVersion = new UmbracoVersion(globalSettings);

            // TODO: Currently we are not passing in any TypeFinderConfig (with ITypeFinderSettings) which we should do, however
            // this is not critical right now and would require loading in some config before boot time so just leaving this as-is for now.
            var typeFinder = new TypeFinder(Current.Logger, new DefaultUmbracoAssemblyProvider(entryAssembly));

            var coreRuntime = GetCoreRuntime(
                Current.Configs,
                umbracoVersion,
                Current.IOHelper,
                Current.Logger,
                Current.Profiler,
                Current.HostingEnvironment,
                Current.BackOfficeInfo,
                typeFinder);

            var factory = coreRuntime.Boot(umbContainer);

            return services;
        }

        private static IRuntime GetCoreRuntime(Configs configs, IUmbracoVersion umbracoVersion, IIOHelper ioHelper, ILogger logger,
            IProfiler profiler, Core.Hosting.IHostingEnvironment hostingEnvironment, IBackOfficeInfo backOfficeInfo,
            ITypeFinder typeFinder)
        {
            var connectionStringConfig = configs.ConnectionStrings()[Constants.System.UmbracoConnectionName];
            var dbProviderFactoryCreator = new SqlServerDbProviderFactoryCreator(
                connectionStringConfig?.ProviderName,
                DbProviderFactories.GetFactory);

            // Determine if we should use the sql main dom or the default
            var globalSettings = configs.Global();
            var connStrings = configs.ConnectionStrings();
            var appSettingMainDomLock = globalSettings.MainDomLock;
            var mainDomLock = appSettingMainDomLock == "SqlMainDomLock"
                ? (IMainDomLock)new SqlMainDomLock(logger, globalSettings, connStrings, dbProviderFactoryCreator)
                : new MainDomSemaphoreLock(logger, hostingEnvironment);

            var mainDom = new MainDom(logger, hostingEnvironment, mainDomLock);

            var coreRuntime = new CoreRuntime(configs, umbracoVersion, ioHelper, logger, profiler, new AspNetCoreBootPermissionsChecker(),
                hostingEnvironment, backOfficeInfo, dbProviderFactoryCreator, mainDom, typeFinder);

            return coreRuntime;
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

            var hostingEnvironment = new AspNetCoreHostingEnvironment(hostingSettings, webHostEnvironment, httpContextAccessor, hostApplicationLifetime);
            var ioHelper = new IOHelper(hostingEnvironment, globalSettings);
            var logger = SerilogLogger.CreateWithDefaultConfiguration(hostingEnvironment,
                new AspNetCoreSessionIdResolver(httpContextAccessor),
                // need to build a new service provider since the one already resolved above doesn't have the IRequestCache yet
                () => services.BuildServiceProvider().GetService<IRequestCache>(), coreDebug, ioHelper,
                new AspNetCoreMarchal());

            var backOfficeInfo = new AspNetCoreBackOfficeInfo(globalSettings);
            var profiler = new LogProfiler(logger);

            Current.Initialize(logger, configs, ioHelper, hostingEnvironment, backOfficeInfo, profiler);

            return services;
        }

        public static IServiceCollection AddUmbracoRuntimeMinifier(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSmidge(configuration.GetSection(Constants.Configuration.ConfigPrefix+"RuntimeMinification"));

            return services;
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
