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

    public DisposableTimer TraceDuration<T>(string startMessage, object[]? startMessageArgs = null)
        => TraceDuration<T>(startMessage, "Completed.", startMessageArgs: startMessageArgs);

    public DisposableTimer TraceDuration<T>(
        string startMessage,
        string completeMessage,
        string? failMessage = null,
        object[]? startMessageArgs = null,
        object[]? endMessageArgs = null,
        object[]? failMessageArgs = null)
        => new(Logger, LogLevel.Information, Profiler, typeof(T), startMessage, completeMessage, failMessage, startMessageArgs, endMessageArgs, failMessageArgs);

    public DisposableTimer TraceDuration(
        Type loggerType,
        string startMessage,
        string completeMessage,
        string? failMessage = null,
        object[]? startMessageArgs = null,
        object[]? endMessageArgs = null,
        object[]? failMessageArgs = null)
        => new(Logger, LogLevel.Information, Profiler, loggerType, startMessage, completeMessage, failMessage, startMessageArgs, endMessageArgs, failMessageArgs);

    public DisposableTimer? DebugDuration<T>(string startMessage, object[]? startMessageArgs = null)
        => Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug)
            ? DebugDuration<T>(startMessage, "Completed.", startMessageArgs: startMessageArgs)
            : null;

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

    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel level)
        => Logger.IsEnabled(level);

    public void LogCritical(Exception exception, string messageTemplate, params object[] propertyValues)
        => Logger.LogCritical(exception, messageTemplate, propertyValues);

    public void LogCritical(string messageTemplate, params object[] propertyValues)
        => Logger.LogCritical(messageTemplate, propertyValues);

    public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
        => Logger.LogError(exception, messageTemplate, propertyValues);

    public void LogError(string messageTemplate, params object[] propertyValues)
        => Logger.LogError(messageTemplate, propertyValues);

    public void LogWarning(string messageTemplate, params object[] propertyValues)
        => Logger.LogWarning(messageTemplate, propertyValues);

    public void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues)
        => Logger.LogWarning(exception, messageTemplate, propertyValues);

    public void LogInformation(string messageTemplate, params object[] propertyValues)
        => Logger.LogInformation(messageTemplate, propertyValues);

    public void LogDebug(string messageTemplate, params object[] propertyValues)
        => Logger.LogDebug(messageTemplate, propertyValues);

    public void LogTrace(string messageTemplate, params object[] propertyValues)
        => Logger.LogTrace(messageTemplate, propertyValues);

    #endregion
}
