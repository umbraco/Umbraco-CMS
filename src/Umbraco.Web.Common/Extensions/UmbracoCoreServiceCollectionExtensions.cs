using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
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


            IHttpContextAccessor httpContextAccessor = new HttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);

            var requestCache = new GenericDictionaryRequestAppCache(() => httpContextAccessor.HttpContext.Items);

            services.AddUmbracoCore(webHostEnvironment, umbContainer, Assembly.GetEntryAssembly(), requestCache, httpContextAccessor, out factory);

            return services;
        }

        /// <summary>
        /// Adds the Umbraco Back Core requirements
        /// </summary>
        /// <param name="services"></param>
        /// <param name="webHostEnvironment"></param>
        /// <param name="umbContainer"></param>
        /// <param name="entryAssembly"></param>
        /// <param name="requestCache"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddUmbracoCore(
            this IServiceCollection services,
            IWebHostEnvironment webHostEnvironment,
            IRegister umbContainer,
            Assembly entryAssembly,
            IRequestCache requestCache,
            IHttpContextAccessor httpContextAccessor,
            out IFactory factory)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            var container = umbContainer;
            if (container is null) throw new ArgumentNullException(nameof(container));
            if (entryAssembly is null) throw new ArgumentNullException(nameof(entryAssembly));

            var serviceProvider = services.BuildServiceProvider();
            var configs = serviceProvider.GetService<Configs>();


            CreateCompositionRoot(services, configs, httpContextAccessor, webHostEnvironment, out var logger,  out var ioHelper, out var hostingEnvironment, out var backOfficeInfo, out var profiler);

            var globalSettings = configs.Global();
            var umbracoVersion = new UmbracoVersion(globalSettings);

            var coreRuntime = GetCoreRuntime(
                configs,
                umbracoVersion,
                ioHelper,
                logger,
                profiler,
                hostingEnvironment,
                backOfficeInfo,
                CreateTypeFinder(logger, profiler, webHostEnvironment, entryAssembly),
                requestCache);

            factory = coreRuntime.Configure(container);

            return services;
        }

        private static ITypeFinder CreateTypeFinder(ILogger logger, IProfiler profiler, IWebHostEnvironment webHostEnvironment, Assembly entryAssembly)
        {
            // TODO: Currently we are not passing in any TypeFinderConfig (with ITypeFinderSettings) which we should do, however
            // this is not critical right now and would require loading in some config before boot time so just leaving this as-is for now.
            var runtimeHashPaths = new RuntimeHashPaths();
            runtimeHashPaths.AddFolder(new DirectoryInfo(Path.Combine(webHostEnvironment.ContentRootPath, "bin")));
            var runtimeHash = new RuntimeHash(new ProfilingLogger(logger, profiler), runtimeHashPaths);
            return new TypeFinder(logger, new DefaultUmbracoAssemblyProvider(entryAssembly), runtimeHash);
        }

        private static IRuntime GetCoreRuntime(
            Configs configs, IUmbracoVersion umbracoVersion, IIOHelper ioHelper, ILogger logger,
            IProfiler profiler, Core.Hosting.IHostingEnvironment hostingEnvironment, IBackOfficeInfo backOfficeInfo,
            ITypeFinder typeFinder, IRequestCache requestCache)
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

            var coreRuntime = new CoreRuntime(
                configs,
                umbracoVersion,
                ioHelper,
                logger,
                profiler,
                new AspNetCoreBootPermissionsChecker(),
                hostingEnvironment,
                backOfficeInfo,
                dbProviderFactoryCreator,
                mainDom,
                typeFinder,
                requestCache);

            return coreRuntime;
        }

        private static IServiceCollection CreateCompositionRoot(IServiceCollection services, Configs configs, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment webHostEnvironment,
            out ILogger logger, out IIOHelper ioHelper, out Core.Hosting.IHostingEnvironment hostingEnvironment,
            out IBackOfficeInfo backOfficeInfo, out IProfiler profiler)
        {
            if (configs == null)
                throw new InvalidOperationException($"Could not resolve type {typeof(Configs)} from the container, ensure {nameof(AddUmbracoConfiguration)} is called before calling {nameof(AddUmbracoCore)}");

            var hostingSettings = configs.Hosting();
            var coreDebug = configs.CoreDebug();
            var globalSettings = configs.Global();

            hostingEnvironment = new AspNetCoreHostingEnvironment(hostingSettings, webHostEnvironment);
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
