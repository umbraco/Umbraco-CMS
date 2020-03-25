using System;
using System.Data.Common;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    // TODO: Move to Umbraco.Web.Common
    public static class UmbracoCoreServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the Umbraco Configuration requirements
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
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
        /// Adds the Umbraco Back Core requirements
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddUmbracoCore(this IServiceCollection services)
        {
            if (!UmbracoServiceProviderFactory.IsActive)
                throw new InvalidOperationException("Ensure to add UseUmbraco() in your Program.cs after ConfigureWebHostDefaults to enable Umbraco's service provider factory");

            var umbContainer = UmbracoServiceProviderFactory.UmbracoContainer;

            return services.AddUmbracoCore(umbContainer, Assembly.GetEntryAssembly());
        }

        /// <summary>
        /// Adds the Umbraco Back Core requirements
        /// </summary>
        /// <param name="services"></param>
        /// <param name="umbContainer"></param>
        /// <param name="entryAssembly"></param>
        /// <returns></returns>
        public static IServiceCollection AddUmbracoCore(this IServiceCollection services, IRegister umbContainer, Assembly entryAssembly)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            if (umbContainer is null) throw new ArgumentNullException(nameof(umbContainer));
            if (entryAssembly is null) throw new ArgumentNullException(nameof(entryAssembly));

            // Special case! The generic host adds a few default services but we need to manually add this one here NOW because
            // we resolve it before the host finishes configuring in the call to CreateCompositionRoot
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            CreateCompositionRoot(services, out var logger, out var configs, out var ioHelper, out var hostingEnvironment, out var backOfficeInfo, out var profiler);

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

            var factory = coreRuntime.Configure(umbContainer);

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

        private static void CreateCompositionRoot(IServiceCollection services,
            out ILogger logger, out Configs configs, out IIOHelper ioHelper, out Core.Hosting.IHostingEnvironment hostingEnvironment,
            out IBackOfficeInfo backOfficeInfo, out IProfiler profiler)
        {
            // TODO: Resolving services before the Host is done configuring this way means that the services resolved
            // are not going to be the same instances that are going to be used within the application!
            var serviceProvider = services.BuildServiceProvider();

            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            var webHostEnvironment = serviceProvider.GetRequiredService<IWebHostEnvironment>();

            configs = serviceProvider.GetService<Configs>();
            if (configs == null)
                throw new InvalidOperationException($"Could not resolve type {typeof(Configs)} from the container, ensure {nameof(AddUmbracoConfiguration)} is called before calling {nameof(AddUmbracoCore)}");

            var hostingSettings = configs.Hosting();
            var coreDebug = configs.CoreDebug();
            var globalSettings = configs.Global();

            hostingEnvironment = new AspNetCoreHostingEnvironment(hostingSettings, webHostEnvironment, httpContextAccessor);
            ioHelper = new IOHelper(hostingEnvironment, globalSettings);
            logger = SerilogLogger.CreateWithDefaultConfiguration(hostingEnvironment,
                new AspNetCoreSessionIdResolver(httpContextAccessor),
                // need to build a new service provider since the one already resolved above doesn't have the IRequestCache yet
                () => services.BuildServiceProvider().GetService<IRequestCache>(), coreDebug, ioHelper,
                new AspNetCoreMarchal());

            backOfficeInfo = new AspNetCoreBackOfficeInfo(globalSettings);
            profiler = new LogProfiler(logger);
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
