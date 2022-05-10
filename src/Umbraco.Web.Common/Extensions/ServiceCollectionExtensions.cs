using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using Serilog.Extensions.Hosting;
using Serilog.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Logging.Serilog;
using Umbraco.Cms.Infrastructure.Logging.Serilog;
using Umbraco.Cms.Web.Common.Hosting;
using Umbraco.Cms.Web.Common.Logging;
using Umbraco.Cms.Web.Common.Logging.Enrichers;
using Constants = Umbraco.Cms.Core.Constants;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;
using ILogger = Serilog.ILogger;

namespace Umbraco.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Create and configure the logger
    /// </summary>
    [Obsolete("Use the extension method that takes an IHostEnvironment instance instead.")]
    public static IServiceCollection AddLogger(
        this IServiceCollection services,
        IHostingEnvironment hostingEnvironment,
        ILoggingConfiguration loggingConfiguration,
        IConfiguration configuration)
    {
        // Create a serilog logger
        var logger = SerilogLogger.CreateWithDefaultConfiguration(hostingEnvironment, loggingConfiguration, configuration, out UmbracoFileConfiguration umbracoFileConfig);
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
            configure.AddSerilog(logger.SerilogLog);
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
    ///     Create and configure the logger.
    /// </summary>
    /// <remarks>
    ///     Additional Serilog services are registered during <see cref="HostBuilderExtensions.ConfigureUmbracoDefaults" />.
    /// </remarks>
    public static IServiceCollection AddLogger(
        this IServiceCollection services,
        IHostEnvironment hostEnvironment,
        IConfiguration configuration)
    {
        // TODO: WEBSITE_RUN_FROM_PACKAGE - can't assume this DIR is writable - we have an IConfiguration instance so a later refactor should be easy enough.
        var loggingDir = hostEnvironment.MapPathContentRoot(Constants.SystemDirectories.LogFiles);
        ILoggingConfiguration loggingConfig = new LoggingConfiguration(loggingDir);

        var umbracoFileConfiguration = new UmbracoFileConfiguration(configuration);

        services.TryAddSingleton(umbracoFileConfiguration);
        services.TryAddSingleton(loggingConfig);
        services.TryAddSingleton<ILogEventEnricher, ApplicationIdEnricher>();

        ///////////////////////////////////////////////
        // Bootstrap logger setup
        ///////////////////////////////////////////////

        LoggerConfiguration serilogConfig = new LoggerConfiguration()
            .MinimalConfiguration(hostEnvironment, loggingConfig, umbracoFileConfiguration)
            .ReadFrom.Configuration(configuration);

        Log.Logger = serilogConfig.CreateBootstrapLogger();

        ///////////////////////////////////////////////
        // Runtime logger setup
        ///////////////////////////////////////////////

        services.AddSingleton(sp =>
        {
            var logger = new RegisteredReloadableLogger(Log.Logger as ReloadableLogger);

            logger.Reload(cfg =>
            {
                cfg.MinimalConfiguration(hostEnvironment, loggingConfig, umbracoFileConfiguration)
                    .ReadFrom.Configuration(configuration)
                    .ReadFrom.Services(sp);

                return cfg;
            });

            return logger;
        });

        services.AddSingleton<ILogger>(sp =>
        {
            ILogger logger = sp.GetRequiredService<RegisteredReloadableLogger>().Logger;
            return logger.ForContext(new NoopEnricher());
        });

        services.AddSingleton<ILoggerFactory>(sp =>
        {
            ILogger logger = sp.GetRequiredService<RegisteredReloadableLogger>().Logger;
            return new SerilogLoggerFactory(logger);
        });

        // Registered to provide two services...
        var diagnosticContext = new DiagnosticContext(Log.Logger);

        // Consumed by e.g. middleware
        services.TryAddSingleton(diagnosticContext);

        // Consumed by user code
        services.TryAddSingleton<IDiagnosticContext>(diagnosticContext);

        return services;
    }

    /// <summary>
    ///     Called to create the <see cref="TypeLoader" /> to assign to the <see cref="IUmbracoBuilder" />
    /// </summary>
    /// <remarks>
    ///     This should never be called in a web project. It is used internally by Umbraco but could be used in unit tests.
    ///     If called in a web project it will have no affect except to create and return a new TypeLoader but this will not
    ///     be the instance in DI.
    /// </remarks>
    [Obsolete("Please use alternative extension method.")]
    public static TypeLoader AddTypeLoader(
        this IServiceCollection services,
        Assembly entryAssembly,
        IHostingEnvironment hostingEnvironment,
        ILoggerFactory loggerFactory,
        AppCaches appCaches,
        IConfiguration configuration,
        IProfiler profiler) =>
        services.AddTypeLoader(entryAssembly, loggerFactory, configuration);

    /// <summary>
    ///     Called to create the <see cref="TypeLoader" /> to assign to the <see cref="IUmbracoBuilder" />
    /// </summary>
    /// <remarks>
    ///     This should never be called in a web project. It is used internally by Umbraco but could be used in unit tests.
    ///     If called in a web project it will have no affect except to create and return a new TypeLoader but this will not
    ///     be the instance in DI.
    /// </remarks>
    public static TypeLoader AddTypeLoader(
        this IServiceCollection services,
        Assembly? entryAssembly,
        ILoggerFactory loggerFactory,
        IConfiguration configuration)
    {
        TypeFinderSettings typeFinderSettings =
            configuration.GetSection(Constants.Configuration.ConfigTypeFinder).Get<TypeFinderSettings>() ??
            new TypeFinderSettings();

        var assemblyProvider = new DefaultUmbracoAssemblyProvider(
            entryAssembly,
            loggerFactory,
            typeFinderSettings.AdditionalEntryAssemblies);

        var typeFinderConfig = new TypeFinderConfig(Options.Create(typeFinderSettings));

        var typeFinder = new TypeFinder(
            loggerFactory.CreateLogger<TypeFinder>(),
            assemblyProvider,
            typeFinderConfig);

        var typeLoader = new TypeLoader(typeFinder, loggerFactory.CreateLogger<TypeLoader>());

        // This will add it ONCE and not again which is what we want since we don't actually want people to call this method
        // in the web project.
        services.TryAddSingleton<ITypeFinder>(typeFinder);
        services.TryAddSingleton(typeLoader);

        return typeLoader;
    }
}
