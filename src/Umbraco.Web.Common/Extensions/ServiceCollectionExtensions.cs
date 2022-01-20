using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Extensions.Hosting;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Logging.Serilog;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Extensions
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// Create and configure the logger
        /// </summary>
        public static IServiceCollection AddLogger(
            this IServiceCollection services,
            IHostingEnvironment hostingEnvironment,
            ILoggingConfiguration loggingConfiguration,
            IConfiguration configuration)
        {
            // Create a serilog logger
            var logger = SerilogLogger.CreateWithDefaultConfiguration(hostingEnvironment, loggingConfiguration, configuration, out var umbracoFileConfig);
            services.AddSingleton(umbracoFileConfig);

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
            services.AddSingleton(loggingConfiguration);

            return services;
        }

        /// <summary>
        /// Called to create the <see cref="TypeLoader"/> to assign to the <see cref="IUmbracoBuilder"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="entryAssembly"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="appCaches"></param>
        /// <param name="configuration"></param>
        /// <param name="profiler"></param>
        /// <returns></returns>
        /// <remarks>
        /// This should never be called in a web project. It is used internally by Umbraco but could be used in unit tests.
        /// If called in a web project it will have no affect except to create and return a new TypeLoader but this will not
        /// be the instance in DI.
        /// </remarks>
        public static TypeLoader AddTypeLoader(
            this IServiceCollection services,
            Assembly entryAssembly,
            IHostingEnvironment hostingEnvironment,
            ILoggerFactory loggerFactory,
            AppCaches appCaches,
            IConfiguration configuration,
            IProfiler profiler)
        {
            TypeFinderSettings typeFinderSettings = configuration.GetSection(Cms.Core.Constants.Configuration.ConfigTypeFinder).Get<TypeFinderSettings>() ?? new TypeFinderSettings();

            var assemblyProvider = new DefaultUmbracoAssemblyProvider(
                entryAssembly,
                loggerFactory,
                typeFinderSettings.AdditionalEntryAssemblies);

            RuntimeHashPaths runtimeHashPaths = new RuntimeHashPaths().AddAssemblies(assemblyProvider);

            var runtimeHash = new RuntimeHash(
                new ProfilingLogger(
                    loggerFactory.CreateLogger<RuntimeHash>(),
                    profiler),
                runtimeHashPaths);

            var typeFinderConfig = new TypeFinderConfig(Options.Create(typeFinderSettings));

            var typeFinder = new TypeFinder(
                loggerFactory.CreateLogger<TypeFinder>(),
                assemblyProvider,
                typeFinderConfig
            );

            var typeLoader = new TypeLoader(
                typeFinder,
                runtimeHash,
                appCaches.RuntimeCache,
                new DirectoryInfo(hostingEnvironment.LocalTempPath),
                loggerFactory.CreateLogger<TypeLoader>(),
                profiler
            );

            // This will add it ONCE and not again which is what we want since we don't actually want people to call this method
            // in the web project.
            services.TryAddSingleton<ITypeFinder>(typeFinder);
            services.TryAddSingleton<TypeLoader>(typeLoader);

            return typeLoader;
        }
    }
}
