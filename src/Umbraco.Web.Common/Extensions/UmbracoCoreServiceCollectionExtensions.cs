using System;
using System.Data.Common;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Smidge;
using Smidge.Nuglify;
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
using Umbraco.Web.Common.AspNetCore;
using Umbraco.Web.Common.Runtime.Profiler;

namespace Umbraco.Web.Common.Extensions
{
    public static class UmbracoCoreServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the Umbraco Configuration requirements
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddUmbracoConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var configsFactory = new AspNetCoreConfigsFactory(configuration);

            var configs = configsFactory.Create();

            services.AddSingleton(configs);

            return services;
        }


        /// <summary>
        /// Adds the Umbraco Back Core requirements
        /// </summary>
        /// <param name="services"></param>
        /// <param name="webHostEnvironment"></param>
        /// <returns></returns>
        public static IServiceCollection AddUmbracoCore(this IServiceCollection services, IWebHostEnvironment webHostEnvironment)
        {
            return services.AddUmbracoCore(webHostEnvironment,out _);
        }

        /// <summary>
        /// Adds the Umbraco Back Core requirements
        /// </summary>
        /// <param name="services"></param>
        /// <param name="webHostEnvironment"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddUmbracoCore(this IServiceCollection services, IWebHostEnvironment webHostEnvironment, out IFactory factory)
        {
            if (!UmbracoServiceProviderFactory.IsActive)
                throw new InvalidOperationException("Ensure to add UseUmbraco() in your Program.cs after ConfigureWebHostDefaults to enable Umbraco's service provider factory");

            var umbContainer = UmbracoServiceProviderFactory.UmbracoContainer;

            services.AddUmbracoCore(webHostEnvironment, umbContainer, Assembly.GetEntryAssembly(), out factory);

            return services;
        }

        /// <summary>
        /// Adds the Umbraco Back Core requirements
        /// </summary>
        /// <param name="services"></param>
        /// <param name="webHostEnvironment"></param>
        /// <param name="umbContainer"></param>
        /// <param name="entryAssembly"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddUmbracoCore(this IServiceCollection services, IWebHostEnvironment webHostEnvironment, IRegister umbContainer, Assembly entryAssembly, out IFactory factory)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            var container = umbContainer;
            if (container is null) throw new ArgumentNullException(nameof(container));
            if (entryAssembly is null) throw new ArgumentNullException(nameof(entryAssembly));

            // Special case! The generic host adds a few default services but we need to manually add this one here NOW because
            // we resolve it before the host finishes configuring in the call to CreateCompositionRoot
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            CreateCompositionRoot(services, webHostEnvironment, out var logger, out var configs, out var ioHelper, out var hostingEnvironment, out var backOfficeInfo, out var profiler);

            var globalSettings = configs.Global();
            var umbracoVersion = new UmbracoVersion(globalSettings);

            // TODO: Currently we are not passing in any TypeFinderConfig (with ITypeFinderSettings) which we should do, however
            // this is not critical right now and would require loading in some config before boot time so just leaving this as-is for now.
            var typeFinder = new TypeFinder(logger, new DefaultUmbracoAssemblyProvider(entryAssembly));

            var coreRuntime = GetCoreRuntime(
                configs,
                umbracoVersion,
                ioHelper,
                logger,
                profiler,
                hostingEnvironment,
                backOfficeInfo,
                typeFinder);

            factory = coreRuntime.Configure(container);

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

            var mainDom = new MainDom(logger, mainDomLock);

            var coreRuntime = new CoreRuntime(configs, umbracoVersion, ioHelper, logger, profiler, new AspNetCoreBootPermissionsChecker(),
                hostingEnvironment, backOfficeInfo, dbProviderFactoryCreator, mainDom, typeFinder);

            return coreRuntime;
        }

        private static IServiceCollection CreateCompositionRoot(IServiceCollection services, IWebHostEnvironment webHostEnvironment,
            out ILogger logger, out Configs configs, out IIOHelper ioHelper, out Core.Hosting.IHostingEnvironment hostingEnvironment,
            out IBackOfficeInfo backOfficeInfo, out IProfiler profiler)
        {
            // TODO: We need to avoid this, surely there's a way? See ContainerTests.BuildServiceProvider_Before_Host_Is_Configured
            var serviceProvider = services.BuildServiceProvider();

            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

            configs = serviceProvider.GetService<Configs>();
            if (configs == null)
                throw new InvalidOperationException($"Could not resolve type {typeof(Configs)} from the container, ensure {nameof(AddUmbracoConfiguration)} is called before calling {nameof(AddUmbracoCore)}");

            var hostingSettings = configs.Hosting();
            var coreDebug = configs.CoreDebug();
            var globalSettings = configs.Global();

            hostingEnvironment = new AspNetCoreHostingEnvironment(hostingSettings, webHostEnvironment, httpContextAccessor);
            ioHelper = new IOHelper(hostingEnvironment, globalSettings);
            logger = SerilogLogger.CreateWithDefaultConfiguration(hostingEnvironment,
                new AspNetCoreSessionManager(httpContextAccessor),
                // TODO: We need to avoid this, surely there's a way? See ContainerTests.BuildServiceProvider_Before_Host_Is_Configured
                () => services.BuildServiceProvider().GetService<IRequestCache>(), coreDebug, ioHelper,
                new AspNetCoreMarchal());

            backOfficeInfo = new AspNetCoreBackOfficeInfo(globalSettings);
            profiler = GetWebProfiler(hostingEnvironment, httpContextAccessor);

            return services;
        }

        public static IServiceCollection AddUmbracoRuntimeMinifier(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSmidge(configuration.GetSection(Constants.Configuration.ConfigRuntimeMinification));
            services.AddSmidgeNuglify();

            return services;
        }

        private static IProfiler GetWebProfiler(Umbraco.Core.Hosting.IHostingEnvironment hostingEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            // create and start asap to profile boot
            if (!hostingEnvironment.IsDebugMode)
            {
                // should let it be null, that's how MiniProfiler is meant to work,
                // but our own IProfiler expects an instance so let's get one
                return new VoidProfiler();
            }

            var webProfiler = new WebProfiler(httpContextAccessor);
            webProfiler.StartBoot();

            return webProfiler;
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
