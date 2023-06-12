namespace Umbraco.Cms.Core.Logging;

/// <summary>
///     Defines the profiling logging service.
/// </summary>
public interface IProfilingLogger
{
    /// <summary>
    ///     Profiles an action and log as information messages.
    /// </summary>
    DisposableTimer TraceDuration<T>(string startMessage, object[]? startMessageArgs = null);

    /// <summary>
    ///     Profiles an action and log as information messages.
    /// </summary>
    DisposableTimer TraceDuration<T>(
        string startMessage,
        string completeMessage,
        string? failMessage = null,
        object[]? startMessageArgs = null,
        object[]? endMessageArgs = null,
        object[]? failMessageArgs = null);

    /// <summary>
    ///     Profiles an action and log as information messages.
    /// </summary>
    DisposableTimer TraceDuration(
        Type loggerType,
        string startMessage,
        string completeMessage,
        string? failMessage = null,
        object[]? startMessageArgs = null,
        object[]? endMessageArgs = null,
        object[]? failMessageArgs = null);

    /// <summary>
    ///     Profiles an action and log as debug messages.
    /// </summary>
    DisposableTimer? DebugDuration<T>(string startMessage, object[]? startMessageArgs = null);

    /// <summary>
    ///     Profiles an action and log as debug messages.
    /// </summary>
    DisposableTimer? DebugDuration<T>(
        string startMessage,
        string completeMessage,
        string? failMessage = null,
        int thresholdMilliseconds = 0,
        object[]? startMessageArgs = null,
        object[]? endMessageArgs = null,
        object[]? failMessageArgs = null);

    /// <summary>
    ///     Profiles an action and log as debug messages.
    /// </summary>
    DisposableTimer? DebugDuration(
        Type loggerType,
        string startMessage,
        string completeMessage,
        string? failMessage = null,
        int thresholdMilliseconds = 0,
        object[]? startMessageArgs = null,
        object[]? endMessageArgs = null,
        object[]? failMessageArgs = null);

    /// <summary>
    /// Checks if the given logLevel is enabled.
    /// </summary>
    /// <param name="logLevel">Level to be checked.</param>
    /// <returns>true if enabled.</returns>
    bool IsEnabled(LogLevel logLevel) => true;
}
