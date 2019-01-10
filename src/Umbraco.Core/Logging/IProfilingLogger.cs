using System;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// Defines the profiling logging service.
    /// </summary>
    public interface IProfilingLogger : ILogger
    {
        /// <summary>
        /// Profiles an action and log as information messages.
        /// </summary>
        DisposableTimer TraceDuration<T>(string startMessage);

        /// <summary>
        /// Profiles an action and log as information messages.
        /// </summary>
        DisposableTimer TraceDuration<T>(string startMessage, string completeMessage, string failMessage = null);

        /// <summary>
        /// Profiles an action and log as information messages.
        /// </summary>
        DisposableTimer TraceDuration(Type loggerType, string startMessage, string completeMessage, string failMessage = null);

        /// <summary>
        /// Profiles an action and log as debug messages.
        /// </summary>
        DisposableTimer DebugDuration<T>(string startMessage);

        /// <summary>
        /// Profiles an action and log as debug messages.
        /// </summary>
        DisposableTimer DebugDuration<T>(string startMessage, string completeMessage, string failMessage = null, int thresholdMilliseconds = 0);

        /// <summary>
        /// Profiles an action and log as debug messages.
        /// </summary>
        DisposableTimer DebugDuration(Type loggerType, string startMessage, string completeMessage, string failMessage = null, int thresholdMilliseconds = 0);
    }
}
