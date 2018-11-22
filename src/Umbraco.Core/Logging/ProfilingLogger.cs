using System;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// Provides debug or trace logging with duration management.
    /// </summary>
    public sealed class ProfilingLogger
    {
        public ILogger Logger { get; }

        public IProfiler Profiler { get; }

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
    }
}
