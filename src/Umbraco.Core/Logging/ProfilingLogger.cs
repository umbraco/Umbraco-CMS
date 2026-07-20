using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Core.Logging;

/// <summary>
///     Provides logging and profiling services.
/// </summary>
public sealed class ProfilingLogger : IProfilingLogger
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ProfilingLogger" /> class.
    /// </summary>
    public ProfilingLogger(ILogger<ProfilingLogger> logger, IProfiler profiler)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Profiler = profiler ?? throw new ArgumentNullException(nameof(profiler));
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProfilingLogger" /> class.
    /// </summary>
    public ProfilingLogger(ILogger logger, IProfiler profiler)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Profiler = profiler ?? throw new ArgumentNullException(nameof(profiler));
    }

    /// <summary>
    ///     Gets the underlying <see cref="ILogger" /> implementation.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    ///     Gets the underlying <see cref="IProfiler" /> implementation.
    /// </summary>
    public IProfiler Profiler { get; }

    /// <inheritdoc/>
    public DisposableTimer TraceDuration<T>(string startMessage, object[]? startMessageArgs = null)
        => TraceDuration<T>(startMessage, "Completed.", startMessageArgs: startMessageArgs);

    /// <inheritdoc/>
    public DisposableTimer TraceDuration<T>(
        string startMessage,
        string completeMessage,
        string? failMessage = null,
        object[]? startMessageArgs = null,
        object[]? endMessageArgs = null,
        object[]? failMessageArgs = null)
        => new(Logger, LogLevel.Information, Profiler, typeof(T), startMessage, completeMessage, failMessage, startMessageArgs, endMessageArgs, failMessageArgs);

    /// <inheritdoc/>
    public DisposableTimer TraceDuration(
        Type loggerType,
        string startMessage,
        string completeMessage,
        string? failMessage = null,
        object[]? startMessageArgs = null,
        object[]? endMessageArgs = null,
        object[]? failMessageArgs = null)
        => new(Logger, LogLevel.Information, Profiler, loggerType, startMessage, completeMessage, failMessage, startMessageArgs, endMessageArgs, failMessageArgs);

    /// <inheritdoc/>
    public DisposableTimer? DebugDuration<T>(string startMessage, object[]? startMessageArgs = null)
        => Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug)
            ? DebugDuration<T>(startMessage, "Completed.", startMessageArgs: startMessageArgs)
            : null;

    /// <inheritdoc/>
    public DisposableTimer? DebugDuration<T>(
        string startMessage,
        string completeMessage,
        string? failMessage = null,
        int thresholdMilliseconds = 0,
        object[]? startMessageArgs = null,
        object[]? endMessageArgs = null,
        object[]? failMessageArgs = null)
        => Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug)
            ? new DisposableTimer(
                Logger,
                LogLevel.Debug,
                Profiler,
                typeof(T),
                startMessage,
                completeMessage,
                failMessage,
                startMessageArgs,
                endMessageArgs,
                failMessageArgs,
                thresholdMilliseconds)
            : null;

    /// <inheritdoc/>
    public DisposableTimer? DebugDuration(
        Type loggerType,
        string startMessage,
        string completeMessage,
        string? failMessage = null,
        int thresholdMilliseconds = 0,
        object[]? startMessageArgs = null,
        object[]? endMessageArgs = null,
        object[]? failMessageArgs = null)
        => Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug)
            ? new DisposableTimer(
                Logger,
                LogLevel.Debug,
                Profiler,
                loggerType,
                startMessage,
                completeMessage,
                failMessage,
                startMessageArgs,
                endMessageArgs,
                failMessageArgs,
                thresholdMilliseconds)
            : null;

    #region ILogger

    /// <summary>
    ///     Checks if the given log level is enabled.
    /// </summary>
    /// <param name="level">The log level to check.</param>
    /// <returns><c>true</c> if the log level is enabled; otherwise, <c>false</c>.</returns>
    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel level)
        => Logger.IsEnabled(level);

    /// <summary>
    ///     Logs a critical message with an exception.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="messageTemplate">The message template.</param>
    /// <param name="propertyValues">The property values to substitute into the template.</param>
    public void LogCritical(Exception exception, string messageTemplate, params object[] propertyValues)
        => Logger.LogCritical(exception, messageTemplate, propertyValues);

    /// <summary>
    ///     Logs a critical message.
    /// </summary>
    /// <param name="messageTemplate">The message template.</param>
    /// <param name="propertyValues">The property values to substitute into the template.</param>
    public void LogCritical(string messageTemplate, params object[] propertyValues)
        => Logger.LogCritical(messageTemplate, propertyValues);

    /// <summary>
    ///     Logs an error message with an exception.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="messageTemplate">The message template.</param>
    /// <param name="propertyValues">The property values to substitute into the template.</param>
    public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
        => Logger.LogError(exception, messageTemplate, propertyValues);

    /// <summary>
    ///     Logs an error message.
    /// </summary>
    /// <param name="messageTemplate">The message template.</param>
    /// <param name="propertyValues">The property values to substitute into the template.</param>
    public void LogError(string messageTemplate, params object[] propertyValues)
        => Logger.LogError(messageTemplate, propertyValues);

    /// <summary>
    ///     Logs a warning message.
    /// </summary>
    /// <param name="messageTemplate">The message template.</param>
    /// <param name="propertyValues">The property values to substitute into the template.</param>
    public void LogWarning(string messageTemplate, params object[] propertyValues)
        => Logger.LogWarning(messageTemplate, propertyValues);

    /// <summary>
    ///     Logs a warning message with an exception.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="messageTemplate">The message template.</param>
    /// <param name="propertyValues">The property values to substitute into the template.</param>
    public void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues)
        => Logger.LogWarning(exception, messageTemplate, propertyValues);

    /// <summary>
    ///     Logs an information message.
    /// </summary>
    /// <param name="messageTemplate">The message template.</param>
    /// <param name="propertyValues">The property values to substitute into the template.</param>
    public void LogInformation(string messageTemplate, params object[] propertyValues)
        => Logger.LogInformation(messageTemplate, propertyValues);

    /// <summary>
    ///     Logs a debug message.
    /// </summary>
    /// <param name="messageTemplate">The message template.</param>
    /// <param name="propertyValues">The property values to substitute into the template.</param>
    public void LogDebug(string messageTemplate, params object[] propertyValues)
        => Logger.LogDebug(messageTemplate, propertyValues);

    /// <summary>
    ///     Logs a trace message.
    /// </summary>
    /// <param name="messageTemplate">The message template.</param>
    /// <param name="propertyValues">The property values to substitute into the template.</param>
    public void LogTrace(string messageTemplate, params object[] propertyValues)
        => Logger.LogTrace(messageTemplate, propertyValues);

    ///<inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.Verbose:
                return Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Trace);
            case LogLevel.Debug:
                return Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug);
            case LogLevel.Information:
                return Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information);
            case LogLevel.Warning:
                return Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Warning);
            case LogLevel.Error:
                return Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Error);
            case LogLevel.Fatal:
                return Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Critical);
            default:
                return true;
        }
    }
    #endregion
}
