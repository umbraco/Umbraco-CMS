using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Extensions.Hosting;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Logging.Serilog;
using Umbraco.Core.Runtime;
using Umbraco.Web.Common.AspNetCore;
using Umbraco.Web.Common.Profiler;

namespace Umbraco.Extensions
{
    public static class UmbracoCoreServiceCollectionExtensions
    {
  
        /// <summary>
        /// Create and configure the logger
        /// </summary>
        public static IServiceCollection AddLogger(
            this IServiceCollection services,
            ILoggingConfiguration loggingConfiguration,
            IConfiguration configuration)
        {
            // Create a serilog logger
            var logger = SerilogLogger.CreateWithDefaultConfiguration(loggingConfiguration, configuration);

            // This is nessasary to pick up all the loggins to MS ILogger.
            Log.Logger = logger.SerilogLog;

            // Wire up all the bits that serilog needs. We need to use our own code since the Serilog ext methods don't cater to our needs since
            // we don't want to use the global serilog `Log` object and we don't have our own ILogger implementation before the HostBuilder runs which
            // is the only other option that these ext methods allow.
            // I have created a PR to make this nicer https://github.com/serilog/serilog-extensions-hosting/pull/19 but we'll need to wait for that.
            // Also see : https://github.com/serilog/serilog-extensions-hosting/blob/dev/src/Serilog.Extensions.Hosting/SerilogHostBuilderExtensions.cs

            services.AddLogging(configure =>
            {
                configure.AddSerilog(logger.SerilogLog, false);
            });

            // This won't (and shouldn't) take ownership of the logger.
            services.AddSingleton(logger.SerilogLog);

            // Registered to provide two services...
            var diagnosticContext = new DiagnosticContext(logger.SerilogLog);

            // Consumed by e.g. middleware
            services.AddSingleton(diagnosticContext);

            // Consumed by user code
            services.AddSingleton<IDiagnosticContext>(diagnosticContext);

            return services;
        }

        internal static IProfiler GetWebProfiler(IConfiguration config)
        {
            var isDebug = config.GetValue<bool>($"{Constants.Configuration.ConfigHosting}:Debug");
            // create and start asap to profile boot
            if (!isDebug)
            {
                // should let it be null, that's how MiniProfiler is meant to work,
                // but our own IProfiler expects an instance so let's get one
                return new VoidProfiler();
            }

            var webProfiler = new WebProfiler();
            webProfiler.StartBoot();

            return webProfiler;
        }

        internal static ITypeFinder AddTypeFinder(this IServiceCollection services, ILoggerFactory loggerFactory, IWebHostEnvironment webHostEnvironment, Assembly entryAssembly, IConfiguration config)
        {
            var profiler = GetWebProfiler(config);

            var typeFinderSettings = config.GetSection(Core.Constants.Configuration.ConfigTypeFinder).Get<TypeFinderSettings>() ?? new TypeFinderSettings();

            var runtimeHashPaths = new RuntimeHashPaths().AddFolder(new DirectoryInfo(Path.Combine(webHostEnvironment.ContentRootPath, "bin")));
            var runtimeHash = new RuntimeHash(new ProfilingLogger(loggerFactory.CreateLogger<ProfilingLogger>(), profiler), runtimeHashPaths);

            var typeFinder =  new TypeFinder(
                loggerFactory.CreateLogger<TypeFinder>(),
                new DefaultUmbracoAssemblyProvider(entryAssembly),
                runtimeHash,
                new TypeFinderConfig(Options.Create(typeFinderSettings))
            );

            services.AddUnique<ITypeFinder>(typeFinder);

            return typeFinder;
        }

        internal static TypeLoader AddTypeLoader(
            this IServiceCollection services,
            Assembly entryAssembly,
            IWebHostEnvironment webHostEnvironment,
            ILoggerFactory loggerFactory,
            AppCaches appCaches,
            IConfiguration configuration)
        {
            var typeFinder = services.AddTypeFinder(loggerFactory, webHostEnvironment, entryAssembly, configuration);
            var hostingSettings = configuration.GetSection(Core.Constants.Configuration.ConfigHosting).Get<HostingSettings>() ?? new HostingSettings();
            var hostingEnvironment = new AspNetCoreHostingEnvironmentWithoutOptionsMonitor(hostingSettings, webHostEnvironment);

            var profilingLogger = new ProfilingLogger(loggerFactory.CreateLogger<ProfilingLogger>(), GetWebProfiler(configuration));

            var typeLoader =  new TypeLoader(
                typeFinder,
                appCaches.RuntimeCache,
                new DirectoryInfo(hostingEnvironment.LocalTempPath),
                loggerFactory.CreateLogger<TypeLoader>(),
                profilingLogger
            );

            services.AddUnique<TypeLoader>(typeLoader);

            return typeLoader;
        }

        internal class AspNetCoreBootPermissionsChecker : IUmbracoBootPermissionChecker
        {
            public void ThrowIfNotPermissions()
            {
                // nothing to check
            }
        }

    }

}
