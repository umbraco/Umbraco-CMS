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

        public void Fatal(Type reporting, Exception exception, string message)
            => Logger.Fatal(reporting, exception, message);

        public void Fatal(Type reporting, Exception exception)
            => Logger.Fatal(reporting, exception);

        public void Fatal(Type reporting, string message)
            => Logger.Fatal(reporting, message);

        public void Fatal(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues)
            => Logger.Fatal(reporting, exception, messageTemplate, propertyValues);

        public void Fatal(Type reporting, string messageTemplate, params object[] propertyValues)
            => Logger.Fatal(reporting, messageTemplate, propertyValues);

        public void Error(Type reporting, Exception exception, string message)
            => Logger.Error(reporting, exception, message);

        public void Error(Type reporting, Exception exception)
            => Logger.Error(reporting, exception);

        public void Error(Type reporting, string message)
            => Logger.Error(reporting, message);

        public void Error(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues)
            => Logger.Error(reporting, exception, messageTemplate, propertyValues);

        public void Error(Type reporting, string messageTemplate, params object[] propertyValues)
            => Logger.Error(reporting, messageTemplate, propertyValues);

        public void Warn(Type reporting, string message)
            => Logger.Warn(reporting, message);

        public void Warn(Type reporting, string messageTemplate, params object[] propertyValues)
            => Logger.Warn(reporting, messageTemplate, propertyValues);

        public void Warn(Type reporting, Exception exception, string message)
            => Logger.Warn(reporting, exception, message);

        public void Warn(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues)
            => Logger.Warn(reporting, exception, messageTemplate, propertyValues);

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

        public void Fatal<T0, T1, T2>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0,
            T1 propertyValue1, T2 propertyValue2)
            => Logger.Fatal<T0, T1, T2>(reporting, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);

        public void Fatal<T0, T1>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
            => Logger.Fatal<T0, T1>(reporting, exception, messageTemplate, propertyValue0, propertyValue1);

        public void Fatal<T0>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0)
            => Logger.Fatal<T0>(reporting, exception, messageTemplate, propertyValue0);

        public void Error<T0, T1, T2>(Type reporting, Exception exception, string messageTemplate,
            T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
            => Logger.Error<T0, T1, T2>(reporting, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);

        public void Error<T0, T1>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
            => Logger.Error<T0, T1>(reporting, exception, messageTemplate, propertyValue0, propertyValue1);

        public void Error<T0>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0)
            => Logger.Error<T0>(reporting, exception, messageTemplate, propertyValue0);

        public void Warn<T0, T1, T2>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
            => Logger.Warn<T0, T1, T2>(reporting, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);

        public void Warn<T0, T1>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
            => Logger.Warn<T0, T1>(reporting, exception, messageTemplate, propertyValue0, propertyValue1);

        public void Warn<T0>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0)
            => Logger.Warn<T0>(reporting, exception, messageTemplate, propertyValue0);

        public void Warn<T0>(Type reporting, string message, T0 propertyValue0) => Logger.Warn(reporting, message, propertyValue0);

        public void Info<T0, T1, T2>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
            => Logger.Info<T0, T1, T2>(reporting, messageTemplate, propertyValue0, propertyValue1, propertyValue2);

        public void Info<T0, T1>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
            => Logger.Info<T0, T1>(reporting, messageTemplate, propertyValue0, propertyValue1);

        public void Info<T0>(Type reporting, string messageTemplate, T0 propertyValue0)
            => Logger.Info<T0>(reporting, messageTemplate, propertyValue0);

        public void Debug<T0, T1, T2>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
            => Logger.Debug<T0, T1, T2>(reporting, messageTemplate, propertyValue0, propertyValue1, propertyValue2);

        public void Debug<T0, T1>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
            => Logger.Debug<T0, T1>(reporting, messageTemplate, propertyValue0, propertyValue1);

        public void Debug<T0>(Type reporting, string messageTemplate, T0 propertyValue0)
            => Logger.Debug<T0>(reporting, messageTemplate, propertyValue0);

        public void Verbose<T0, T1, T2>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
            => Logger.Verbose<T0, T1, T2>(reporting, messageTemplate, propertyValue0, propertyValue1, propertyValue2);

        public void Verbose<T0, T1>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
            => Logger.Verbose<T0, T1>(reporting, messageTemplate, propertyValue0, propertyValue1);

        public void Verbose<T0>(Type reporting, string messageTemplate, T0 propertyValue0)
            => Logger.Verbose<T0>(reporting, messageTemplate, propertyValue0);

        #endregion
    }
}
