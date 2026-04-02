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
    /// <summary>
    /// Initializes a new instance of the <see cref="SerilogLogger"/> class with the specified <see cref="LoggerConfiguration"/>.
    /// </summary>
    /// <param name="logConfig">The <see cref="LoggerConfiguration"/> to configure the Serilog logger.</param>
    public SerilogLogger(LoggerConfiguration logConfig) =>

        // Configure Serilog static global logger with config passed in
        SerilogLog = logConfig.CreateLogger();

    /// <summary>
    /// Gets the underlying Serilog <see cref="ILogger"/> instance used for logging.
    /// </summary>
    public ILogger SerilogLog { get; }

    /// <summary>
    /// Releases all resources used by the <see cref="SerilogLogger"/> by disposing the underlying Serilog logger instance.
    /// </summary>
    public void Dispose() => (SerilogLog as IDisposable)?.Dispose();

    /// <summary>
    /// Determines whether logging is enabled for the specified reporting type and log level.
    /// </summary>
    /// <param name="reporting">The type that is reporting the log message.</param>
    /// <param name="level">The level of the log message.</param>
    /// <returns><c>true</c> if logging is enabled for the given type and level; otherwise, <c>false</c>.</returns>
    public bool IsEnabled(Type reporting, LogLevel level)
        => LoggerFor(reporting).IsEnabled(MapLevel(level));

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

    /// <summary>
    /// Logs a fatal error message and exception for the specified reporting type.
    /// </summary>
    /// <param name="reporting">The type on whose behalf the fatal error is being logged.</param>
    /// <param name="exception">The exception associated with the fatal error.</param>
    /// <param name="message">The message describing the fatal error.</param>
    public void Fatal(Type reporting, Exception exception, string message)
    {
        ILogger logger = LoggerFor(reporting);
        logger.Fatal(exception, message);
    }

    /// <summary>
    /// Logs a fatal error message and exception for the specified reporting type.
    /// </summary>
    /// <param name="reporting">The type on whose behalf the fatal error is being logged.</param>
    /// <param name="exception">The exception associated with the fatal error.</param>
    public void Fatal(Type reporting, Exception exception)
    {
        ILogger logger = LoggerFor(reporting);
        var message = "Exception.";
        logger.Fatal(exception, message);
    }

    /// <summary>
    /// Logs a fatal error message using the specified reporting type.
    /// </summary>
    /// <param name="reporting">The <see cref="Type"/> that is reporting the fatal error.</param>
    /// <param name="message">The fatal error message to log.</param>
    public void Fatal(Type reporting, string message) => LoggerFor(reporting).Fatal(message);

    /// <summary>
    /// Logs a fatal error message for the specified reporting type using the provided message template and property values.
    /// </summary>
    /// <param name="reporting">The <see cref="Type"/> that is reporting the fatal error.</param>
    /// <param name="messageTemplate">A message template describing the fatal error, which can include placeholders for property values.</param>
    /// <param name="propertyValues">An array of property values to format into the message template.</param>

    public void Fatal(Type reporting, string messageTemplate, params object[] propertyValues) =>
        LoggerFor(reporting).Fatal(messageTemplate, propertyValues);

    /// <summary>
    /// Logs a fatal error message for the specified reporting type, including an exception and a formatted message template.
    /// </summary>
    /// <param name="reporting">The <see cref="Type"/> that is reporting the fatal error.</param>
    /// <param name="exception">The <see cref="Exception"/> associated with the fatal error.</param>
    /// <param name="messageTemplate">A message template describing the fatal error, which can include placeholders for property values.</param>
    /// <param name="propertyValues">An array of property values to format the message template.</param>
    public void Fatal(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues)
    {
        ILogger logger = LoggerFor(reporting);
        logger.Fatal(exception, messageTemplate, propertyValues);
    }

    /// <summary>
    /// Logs an error message and exception for the specified reporting type.
    /// </summary>
    /// <param name="reporting">The <see cref="Type"/> of the component reporting the error.</param>
    /// <param name="exception">The <see cref="Exception"/> to log.</param>
    /// <param name="message">The error message to log.</param>
    public void Error(Type reporting, Exception exception, string message)
    {
        ILogger logger = LoggerFor(reporting);
        logger.Error(exception, message);
    }

    /// <summary>
    /// Logs an error message and exception for the specified reporting component type.
    /// </summary>
    /// <param name="reporting">The <see cref="Type"/> representing the component reporting the error.</param>
    /// <param name="exception">The <see cref="Exception"/> instance to log.</param>
    /// <param name="message">The error message describing the context of the exception.</param>
    public void Error(Type reporting, Exception exception)
    {
        ILogger logger = LoggerFor(reporting);
        var message = "Exception";
        logger.Error(exception, message);
    }

    /// <summary>
    /// Logs an error message for the specified reporting type without an associated exception.
    /// </summary>
    /// <param name="reporting">The <see cref="Type"/> (usually the calling class) that is reporting the error.</param>
    /// <param name="message">The error message to log.</param>
    public void Error(Type reporting, string message) => LoggerFor(reporting).Error(message);

    /// <summary>
    /// Logs an error message for the specified reporting type, including an exception and a message.
    /// </summary>
    /// <param name="reporting">The <see cref="Type"/> that is reporting the error.</param>
    /// <param name="exception">The <see cref="Exception"/> associated with the error.</param>
    /// <param name="message">The error message to log.</param>
    public void Error(Type reporting, string messageTemplate, params object[] propertyValues) =>
        LoggerFor(reporting).Error(messageTemplate, propertyValues);

    /// <summary>
    /// Logs an error message for the specified reporting type, including an associated exception and optional structured property values for message template formatting.
    /// </summary>
    /// <param name="reporting">The <see cref="Type"/> that is reporting the error.</param>
    /// <param name="exception">The <see cref="Exception"/> to log.</param>
    /// <param name="messageTemplate">A message template describing the error, which may include placeholders for property values.</param>
    /// <param name="propertyValues">An array of objects to format the message template.</param>
    public void Error(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues)
    {
        ILogger logger = LoggerFor(reporting);
        logger.Error(exception, messageTemplate, propertyValues);
    }

    /// <summary>
    /// Logs a warning message for the specified reporting type.
    /// </summary>
    /// <param name="reporting">The type that is issuing the warning.</param>
    /// <param name="message">The warning message to log.</param>
    public void Warn(Type reporting, string message) => LoggerFor(reporting).Warning(message);

    /// <summary>
    /// Logs a warning message for the specified reporting type.
    /// </summary>
    /// <param name="reporting">The <see cref="Type"/> that is reporting the warning.</param>
    /// <param name="message">The warning message to log. May contain format placeholders for property values.</param>
    /// <param name="propertyValues">Optional property values to format into the message.</param>
    public void Warn(Type reporting, string message, params object[] propertyValues) =>
        LoggerFor(reporting).Warning(message, propertyValues);

    /// <summary>
    /// Logs a warning message and exception for the specified reporting type.
    /// </summary>
    /// <param name="reporting">The type that is issuing the warning.</param>
    /// <param name="exception">The exception to log with the warning message.</param>
    /// <param name="message">The warning message to log.</param>
    public void Warn(Type reporting, Exception exception, string message) =>
        LoggerFor(reporting).Warning(exception, message);

    /// <summary>
    /// Logs a warning message for the specified reporting type, including an exception and a formatted message template.
    /// </summary>
    /// <param name="reporting">The <see cref="Type"/> that is reporting the warning.</param>
    /// <param name="exception">The <see cref="Exception"/> associated with the warning.</param>
    /// <param name="messageTemplate">A message template describing the warning, which can include placeholders for formatting.</param>
    /// <param name="propertyValues">An array of objects to format the message template.</param>
    public void Warn(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues) =>
        LoggerFor(reporting).Warning(exception, messageTemplate, propertyValues);

    /// <summary>
    /// Logs a message at the Information level for the specified reporting type.
    /// </summary>
    /// <param name="reporting">The <see cref="Type"/> whose logger will be used to log the message.</param>
    /// <param name="message">The informational message to log.</param>
    public void Info(Type reporting, string message) => LoggerFor(reporting).Information(message);

    /// <summary>
    /// Logs an informational message for the specified reporting type using a message template and optional property values.
    /// </summary>
    /// <param name="reporting">The <see cref="Type"/> that is reporting the log message.</param>
    /// <param name="messageTemplate">A message template describing the log message, which can include placeholders for property values.</param>
    /// <param name="propertyValues">An array of objects to format into the message template placeholders.</param>
    public void Info(Type reporting, string messageTemplate, params object[] propertyValues) =>
        LoggerFor(reporting).Information(messageTemplate, propertyValues);

    /// <summary>
    /// Logs a debug-level message using the logger associated with the specified reporting type.
    /// </summary>
    /// <param name="reporting">The <see cref="Type"/> that is reporting the debug message.</param>
    /// <param name="message">The debug message to log.</param>
    public void Debug(Type reporting, string message) => LoggerFor(reporting).Debug(message);

    /// <summary>
    /// Logs a debug-level message for the specified reporting type using a message template and optional property values.
    /// </summary>
    /// <param name="reporting">The <see cref="Type"/> that is reporting the log message.</param>
    /// <param name="messageTemplate">A message template describing the log message, which can include placeholders for property values.</param>
    /// <param name="propertyValues">An optional array of property values to format the message template.</param>
    public void Debug(Type reporting, string messageTemplate, params object[] propertyValues) =>
        LoggerFor(reporting).Debug(messageTemplate, propertyValues);

    /// <summary>
    /// Logs a verbose-level message for the specified reporting type.
    /// </summary>
    /// <param name="reporting">The <see cref="Type"/> that is reporting the message.</param>
    /// <param name="message">The verbose message to log.</param>
    public void Verbose(Type reporting, string message) => LoggerFor(reporting).Verbose(message);

    /// <summary>
    /// Logs a verbose-level message for the specified reporting type using a message template and optional property values.
    /// </summary>
    /// <param name="reporting">The <see cref="Type"/> that is reporting the log message.</param>
    /// <param name="messageTemplate">A message template describing the log message, which can include placeholders for property values.</param>
    /// <param name="propertyValues">An optional array of property values to format the message template.</param>
    public void Verbose(Type reporting, string messageTemplate, params object[] propertyValues) =>
        LoggerFor(reporting).Verbose(messageTemplate, propertyValues);
}
