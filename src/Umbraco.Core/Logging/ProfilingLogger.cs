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
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (profiler == null) throw new ArgumentNullException(nameof(profiler));
            Logger = logger;
            Profiler = profiler;
        }

        public DisposableTimer TraceDuration<T>(string startMessage)
        {
            return TraceDuration<T>(startMessage, "Completed.");
        }

        public DisposableTimer TraceDuration<T>(string startMessage, string completeMessage, string failMessage = null)
        {
            return new DisposableTimer(Logger, DisposableTimer.LogType.Info, Profiler, typeof(T), startMessage, completeMessage, failMessage);
        }

        public DisposableTimer TraceDuration(Type loggerType, string startMessage, string completeMessage, string failMessage = null)
        {
            return new DisposableTimer(Logger, DisposableTimer.LogType.Info, Profiler, loggerType, startMessage, completeMessage, failMessage);
        }

        public DisposableTimer DebugDuration<T>(string startMessage)
        {
            return DebugDuration<T>(startMessage, "Completed.");
        }

        public DisposableTimer DebugDuration<T>(string startMessage, string completeMessage, string failMessage = null, int thresholdMilliseconds = 0)
        {
            return new DisposableTimer(Logger, DisposableTimer.LogType.Debug, Profiler, typeof(T), startMessage, completeMessage, failMessage, thresholdMilliseconds);
        }

        public DisposableTimer DebugDuration(Type loggerType, string startMessage, string completeMessage, string failMessage = null, int thresholdMilliseconds = 0)
        {
            return new DisposableTimer(Logger, DisposableTimer.LogType.Debug, Profiler, loggerType, startMessage, completeMessage, failMessage, thresholdMilliseconds);
        }
    }
}
