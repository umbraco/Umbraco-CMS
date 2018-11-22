using System;
using Umbraco.Core.Profiling;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// Used to create DisposableTimer instances for debugging or tracing durations
    /// </summary>
    public sealed class ProfilingLogger
    {
        public ILogger Logger { get; private set; }
        public IProfiler Profiler { get; private set; }

        public ProfilingLogger(ILogger logger, IProfiler profiler)
        {
            Logger = logger;
            Profiler = profiler;
            if (logger == null) throw new ArgumentNullException("logger");
            if (profiler == null) throw new ArgumentNullException("profiler");           
        }

        public DisposableTimer TraceDuration<T>(string startMessage, string completeMessage)
        {
            return new DisposableTimer(Logger, DisposableTimer.LogType.Info, Profiler, typeof(T), startMessage, completeMessage);
        }

        public DisposableTimer TraceDuration<T>(string startMessage)
        {
            return new DisposableTimer(Logger, DisposableTimer.LogType.Info, Profiler, typeof(T), startMessage, "Complete");
        }

        public DisposableTimer TraceDuration(Type loggerType, string startMessage, string completeMessage)
        {
            return new DisposableTimer(Logger, DisposableTimer.LogType.Info, Profiler, loggerType, startMessage, completeMessage);
        }

        public DisposableTimer DebugDuration<T>(string startMessage, string completeMessage)
        {
            return new DisposableTimer(Logger, DisposableTimer.LogType.Debug, Profiler, typeof(T), startMessage, completeMessage);
        }

        public DisposableTimer DebugDuration<T>(string startMessage)
        {
            return new DisposableTimer(Logger, DisposableTimer.LogType.Debug, Profiler, typeof(T), startMessage, "Complete");
        }

        public DisposableTimer DebugDuration(Type loggerType, string startMessage, string completeMessage)
        {
            return new DisposableTimer(Logger, DisposableTimer.LogType.Debug, Profiler, loggerType, startMessage, completeMessage);
        }
    }
}