using System;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// Provides logging and profiling services.
    /// </summary>
    public sealed class ProfilingLogger : IProfilingLogger
    {
        /// <summary>
        /// Gets the underlying <see cref="ILogger"/> implementation.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Gets the underlying <see cref="IProfiler"/> implementation.
        /// </summary>
        public IProfiler Profiler { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfilingLogger"/> class.
        /// </summary>
        public ProfilingLogger(ILogger logger, IProfiler profiler)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Profiler = profiler ?? throw new ArgumentNullException(nameof(profiler));
        }

        public DisposableTimer TraceDuration<T>(string startMessage)
        {
            return TraceDuration<T>(startMessage, "Completed.");
        }

        public DisposableTimer TraceDuration<T>(string startMessage, string completeMessage, string failMessage = null)
        {
            return new DisposableTimer(Logger, LogLevel.Information, Profiler, typeof(T), startMessage, completeMessage, failMessage);
        }

        public DisposableTimer TraceDuration(Type loggerType, string startMessage, string completeMessage, string failMessage = null)
        {
            return new DisposableTimer(Logger, LogLevel.Information, Profiler, loggerType, startMessage, completeMessage, failMessage);
        }

        public DisposableTimer DebugDuration<T>(string startMessage)
        {
            return Logger.IsEnabled<T>(LogLevel.Debug)
                ? DebugDuration<T>(startMessage, "Completed.")
                : null;
        }

        public DisposableTimer DebugDuration<T>(string startMessage, string completeMessage, string failMessage = null, int thresholdMilliseconds = 0)
        {
            return Logger.IsEnabled<T>(LogLevel.Debug)
                ? new DisposableTimer(Logger, LogLevel.Debug, Profiler, typeof(T), startMessage, completeMessage, failMessage, thresholdMilliseconds)
                : null;
        }

        public DisposableTimer DebugDuration(Type loggerType, string startMessage, string completeMessage, string failMessage = null, int thresholdMilliseconds = 0)
        {
            return Logger.IsEnabled(loggerType, LogLevel.Debug)
                ? new DisposableTimer(Logger, LogLevel.Debug, Profiler, loggerType, startMessage, completeMessage, failMessage, thresholdMilliseconds)
                : null;
        }

        #region ILogger

        public bool IsEnabled(Type reporting, LogLevel level)
            => Logger.IsEnabled(reporting, level);

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

        public void Info(Type reporting, string message)
            => Logger.Info(reporting, message);

        public void Info(Type reporting, string messageTemplate, params object[] propertyValues)
            => Logger.Info(reporting, messageTemplate, propertyValues);

        public void Debug(Type reporting, string message)
            => Logger.Debug(reporting, message);

        public void Debug(Type reporting, string messageTemplate, params object[] propertyValues)
            => Logger.Debug(reporting, messageTemplate, propertyValues);

        public void Verbose(Type reporting, string message)
            => Logger.Verbose(reporting, message);

        public void Verbose(Type reporting, string messageTemplate, params object[] propertyValues)
            => Logger.Verbose(reporting, messageTemplate, propertyValues);

        #endregion
    }
}
