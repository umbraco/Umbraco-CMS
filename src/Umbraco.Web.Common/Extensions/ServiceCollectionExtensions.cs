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
        LoggingSettings loggerSettings = GetLoggerSettings(configuration);

        var loggingDir = loggerSettings.GetAbsoluteLoggingPath(hostEnvironment);
        ILoggingConfiguration loggingConfig = new LoggingConfiguration(loggingDir, loggerSettings.FileNameFormat, loggerSettings.FileNameFormatArguments);

        var umbracoFileConfiguration = new UmbracoFileConfiguration(configuration);

        services.TryAddSingleton(umbracoFileConfiguration);
        services.TryAddSingleton(loggingConfig);
        services.TryAddSingleton<ILogEventEnricher, ApplicationIdEnricher>();

        ///////////////////////////////////////////////
        // Bootstrap logger setup
        ///////////////////////////////////////////////

        Func<LoggerConfiguration, LoggerConfiguration> serilogConfig = cfg => cfg
            .MinimalConfiguration(hostEnvironment, loggingConfig, umbracoFileConfiguration)
            .ReadFrom.Configuration(configuration);

        if (Log.Logger is ReloadableLogger reloadableLogger)
        {
            reloadableLogger.Reload(serilogConfig);
        }
        else
        {
            Log.Logger = serilogConfig(new LoggerConfiguration()).CreateBootstrapLogger();
        }

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

    private static LoggingSettings GetLoggerSettings(IConfiguration configuration)
    {
        var loggerSettings = new LoggingSettings();
        configuration.GetSection(Constants.Configuration.ConfigLogging).Bind(loggerSettings);
        return loggerSettings;
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
            typeFinderSettings.AdditionalAssemblyExclusionEntries.ToArray(),
            typeFinderConfig);

        var typeLoader = new TypeLoader(typeFinder, loggerFactory.CreateLogger<TypeLoader>());

        // This will add it ONCE and not again which is what we want since we don't actually want people to call this method
        // in the web project.
        services.TryAddSingleton<ITypeFinder>(typeFinder);
        services.TryAddSingleton(typeLoader);

        return typeLoader;
    }
}
