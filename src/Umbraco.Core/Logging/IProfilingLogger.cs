namespace Umbraco.Cms.Core.Logging;

/// <summary>
///     Defines the profiling logging service.
/// </summary>
public interface IProfilingLogger
{
    /// <summary>
    ///     Profiles an action and log as information messages.
    /// </summary>
    /// <typeparam name="T">The type to use as the logger category.</typeparam>
    /// <param name="startMessage">The message to log when the timer starts.</param>
    /// <param name="startMessageArgs">Arguments for the start message template.</param>
    /// <returns>A <see cref="DisposableTimer"/> that logs completion when disposed.</returns>
    DisposableTimer TraceDuration<T>(string startMessage, object[]? startMessageArgs = null);

    /// <summary>
    ///     Profiles an action and log as information messages.
    /// </summary>
    /// <typeparam name="T">The type to use as the logger category.</typeparam>
    /// <param name="startMessage">The message to log when the timer starts.</param>
    /// <param name="completeMessage">The message to log when the timer ends successfully.</param>
    /// <param name="failMessage">The message to log if the operation fails.</param>
    /// <param name="startMessageArgs">Arguments for the start message template.</param>
    /// <param name="endMessageArgs">Arguments for the end message template.</param>
    /// <param name="failMessageArgs">Arguments for the fail message template.</param>
    /// <returns>A <see cref="DisposableTimer"/> that logs completion when disposed.</returns>
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
    /// <param name="loggerType">The type to use as the logger category.</param>
    /// <param name="startMessage">The message to log when the timer starts.</param>
    /// <param name="completeMessage">The message to log when the timer ends successfully.</param>
    /// <param name="failMessage">The message to log if the operation fails.</param>
    /// <param name="startMessageArgs">Arguments for the start message template.</param>
    /// <param name="endMessageArgs">Arguments for the end message template.</param>
    /// <param name="failMessageArgs">Arguments for the fail message template.</param>
    /// <returns>A <see cref="DisposableTimer"/> that logs completion when disposed.</returns>
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
    /// <typeparam name="T">The type to use as the logger category.</typeparam>
    /// <param name="startMessage">The message to log when the timer starts.</param>
    /// <param name="startMessageArgs">Arguments for the start message template.</param>
    /// <returns>A <see cref="DisposableTimer"/> that logs completion when disposed, or <c>null</c> if debug logging is disabled.</returns>
    DisposableTimer? DebugDuration<T>(string startMessage, object[]? startMessageArgs = null);

    /// <summary>
    ///     Profiles an action and log as debug messages.
    /// </summary>
    /// <typeparam name="T">The type to use as the logger category.</typeparam>
    /// <param name="startMessage">The message to log when the timer starts.</param>
    /// <param name="completeMessage">The message to log when the timer ends successfully.</param>
    /// <param name="failMessage">The message to log if the operation fails.</param>
    /// <param name="thresholdMilliseconds">Minimum elapsed time in milliseconds before logging the end message.</param>
    /// <param name="startMessageArgs">Arguments for the start message template.</param>
    /// <param name="endMessageArgs">Arguments for the end message template.</param>
    /// <param name="failMessageArgs">Arguments for the fail message template.</param>
    /// <returns>A <see cref="DisposableTimer"/> that logs completion when disposed, or <c>null</c> if debug logging is disabled.</returns>
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
    /// <param name="loggerType">The type to use as the logger category.</param>
    /// <param name="startMessage">The message to log when the timer starts.</param>
    /// <param name="completeMessage">The message to log when the timer ends successfully.</param>
    /// <param name="failMessage">The message to log if the operation fails.</param>
    /// <param name="thresholdMilliseconds">Minimum elapsed time in milliseconds before logging the end message.</param>
    /// <param name="startMessageArgs">Arguments for the start message template.</param>
    /// <param name="endMessageArgs">Arguments for the end message template.</param>
    /// <param name="failMessageArgs">Arguments for the fail message template.</param>
    /// <returns>A <see cref="DisposableTimer"/> that logs completion when disposed, or <c>null</c> if debug logging is disabled.</returns>
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
    ///     Checks if the given logLevel is enabled.
    /// </summary>
    /// <param name="logLevel">Level to be checked.</param>
    /// <returns><c>true</c> if enabled; otherwise, <c>false</c>.</returns>
    bool IsEnabled(LogLevel logLevel) => true;
}
