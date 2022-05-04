using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Infrastructure.Logging.Serilog;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Logging.Serilog;

/// <summary>
///     Implements MS ILogger on top of Serilog.
/// </summary>
public class SerilogLogger : IDisposable
{
    public SerilogLogger(LoggerConfiguration logConfig) =>
        //Configure Serilog static global logger with config passed in
        SerilogLog = logConfig.CreateLogger();

    public ILogger SerilogLog { get; }

    public void Dispose() => SerilogLog.DisposeIfDisposable();

    [Obsolete]
    public static SerilogLogger CreateWithDefaultConfiguration(
        IHostingEnvironment hostingEnvironment,
        ILoggingConfiguration loggingConfiguration,
        IConfiguration configuration) =>
        CreateWithDefaultConfiguration(hostingEnvironment, loggingConfiguration, configuration, out _);

    /// <summary>
    ///     Creates a logger with some pre-defined configuration and remainder from config file
    /// </summary>
    /// <remarks>Used by UmbracoApplicationBase to get its logger.</remarks>
    [Obsolete]
    public static SerilogLogger CreateWithDefaultConfiguration(
        IHostingEnvironment hostingEnvironment,
        ILoggingConfiguration loggingConfiguration,
        IConfiguration configuration,
        out UmbracoFileConfiguration umbracoFileConfig)
    {
        LoggerConfiguration? serilogConfig = new LoggerConfiguration()
            .MinimalConfiguration(hostingEnvironment, loggingConfiguration, configuration, out umbracoFileConfig)
            .ReadFrom.Configuration(configuration);

        return new SerilogLogger(serilogConfig);
    }

    /// <summary>
    ///     Gets a contextualized logger.
    /// </summary>
    private ILogger LoggerFor(Type reporting)
        => SerilogLog.ForContext(reporting);

    /// <summary>
    ///     Maps Umbraco's log level to Serilog's.
    /// </summary>
    private LogEventLevel MapLevel(LogLevel level)
    {
        switch (level)
        {
            case LogLevel.Debug:
                return LogEventLevel.Debug;
            case LogLevel.Error:
                return LogEventLevel.Error;
            case LogLevel.Fatal:
                return LogEventLevel.Fatal;
            case LogLevel.Information:
                return LogEventLevel.Information;
            case LogLevel.Verbose:
                return LogEventLevel.Verbose;
            case LogLevel.Warning:
                return LogEventLevel.Warning;
        }

        throw new NotSupportedException($"LogLevel \"{level}\" is not supported.");
    }

    /// <inheritdoc />
    public bool IsEnabled(Type reporting, LogLevel level)
        => LoggerFor(reporting).IsEnabled(MapLevel(level));

    /// <inheritdoc />
    public void Fatal(Type reporting, Exception exception, string message)
    {
        ILogger logger = LoggerFor(reporting);
        logger.Fatal(exception, message);
    }

    /// <inheritdoc />
    public void Fatal(Type reporting, Exception exception)
    {
        ILogger logger = LoggerFor(reporting);
        var message = "Exception.";
        logger.Fatal(exception, message);
    }

    /// <inheritdoc />
    public void Fatal(Type reporting, string message) => LoggerFor(reporting).Fatal(message);

    /// <inheritdoc />
    public void Fatal(Type reporting, string messageTemplate, params object[] propertyValues) =>
        LoggerFor(reporting).Fatal(messageTemplate, propertyValues);

    /// <inheritdoc />
    public void Fatal(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues)
    {
        ILogger logger = LoggerFor(reporting);
        logger.Fatal(exception, messageTemplate, propertyValues);
    }

    /// <inheritdoc />
    public void Error(Type reporting, Exception exception, string message)
    {
        ILogger logger = LoggerFor(reporting);
        logger.Error(exception, message);
    }

    /// <inheritdoc />
    public void Error(Type reporting, Exception exception)
    {
        ILogger logger = LoggerFor(reporting);
        var message = "Exception";
        logger.Error(exception, message);
    }

    /// <inheritdoc />
    public void Error(Type reporting, string message) => LoggerFor(reporting).Error(message);

    /// <inheritdoc />
    public void Error(Type reporting, string messageTemplate, params object[] propertyValues) =>
        LoggerFor(reporting).Error(messageTemplate, propertyValues);

    /// <inheritdoc />
    public void Error(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues)
    {
        ILogger logger = LoggerFor(reporting);
        logger.Error(exception, messageTemplate, propertyValues);
    }

    /// <inheritdoc />
    public void Warn(Type reporting, string message) => LoggerFor(reporting).Warning(message);

    /// <inheritdoc />
    public void Warn(Type reporting, string message, params object[] propertyValues) =>
        LoggerFor(reporting).Warning(message, propertyValues);

    /// <inheritdoc />
    public void Warn(Type reporting, Exception exception, string message) =>
        LoggerFor(reporting).Warning(exception, message);

    /// <inheritdoc />
    public void Warn(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues) =>
        LoggerFor(reporting).Warning(exception, messageTemplate, propertyValues);

    /// <inheritdoc />
    public void Info(Type reporting, string message) => LoggerFor(reporting).Information(message);

    /// <inheritdoc />
    public void Info(Type reporting, string messageTemplate, params object[] propertyValues) =>
        LoggerFor(reporting).Information(messageTemplate, propertyValues);

    /// <inheritdoc />
    public void Debug(Type reporting, string message) => LoggerFor(reporting).Debug(message);

    /// <inheritdoc />
    public void Debug(Type reporting, string messageTemplate, params object[] propertyValues) =>
        LoggerFor(reporting).Debug(messageTemplate, propertyValues);

    /// <inheritdoc />
    public void Verbose(Type reporting, string message) => LoggerFor(reporting).Verbose(message);

    /// <inheritdoc />
    public void Verbose(Type reporting, string messageTemplate, params object[] propertyValues) =>
        LoggerFor(reporting).Verbose(messageTemplate, propertyValues);
}
