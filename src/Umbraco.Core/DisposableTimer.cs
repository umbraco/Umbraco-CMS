using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Web;
using Umbraco.Core.Logging;
using Umbraco.Core.Profiling;

namespace Umbraco.Core
{
	/// <summary>
	/// Starts the timer and invokes a  callback upon disposal. Provides a simple way of timing an operation by wrapping it in a <code>using</code> (C#) statement.
	/// </summary>
	public class DisposableTimer : DisposableObjectSlim
    {
	    private readonly ILogger _logger;
	    private readonly LogType? _logType;
	    private readonly IProfiler _profiler;
	    private readonly Type _loggerType;
	    private readonly string _endMessage;
	    private readonly IDisposable _profilerStep;
	    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
		private readonly Action<long> _callback;

	    internal enum LogType
	    {
	        Debug, Info
	    }

        internal DisposableTimer(ILogger logger, LogType logType, IProfiler profiler, Type loggerType, string startMessage, string endMessage)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (loggerType == null) throw new ArgumentNullException("loggerType");

            _logger = logger;
            _logType = logType;
            _profiler = profiler;
            _loggerType = loggerType;
            _endMessage = endMessage;
            
            switch (logType)
            {
                case LogType.Debug:
                    logger.Debug(loggerType, startMessage);
                    break;
                case LogType.Info:
                    logger.Info(loggerType, startMessage);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("logType");
            }
            
            if (profiler != null)
            {
                _profilerStep = profiler.Step(loggerType, startMessage);
            }
        }

	    protected internal DisposableTimer(Action<long> callback)
	    {
	        if (callback == null) throw new ArgumentNullException("callback");
	        _callback = callback;
	    }

	    public Stopwatch Stopwatch
		{
			get { return _stopwatch; }
		}

		/// <summary>
		/// Starts the timer and invokes the specified callback upon disposal.
		/// </summary>
		/// <param name="callback">The callback.</param>
		/// <returns></returns>
		[Obsolete("Use either TraceDuration or DebugDuration instead of using Start")]
		public static DisposableTimer Start(Action<long> callback)
		{
			return new DisposableTimer(callback);
		}

        #region TraceDuration

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use the other methods that specify strings instead of Func")]
        public static DisposableTimer TraceDuration<T>(Func<string> startMessage, Func<string> completeMessage)
        {
            return TraceDuration(typeof(T), startMessage, completeMessage);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use the other methods that specify strings instead of Func")]
        public static DisposableTimer TraceDuration(Type loggerType, Func<string> startMessage, Func<string> completeMessage)
        {
            return new DisposableTimer(
                LoggerResolver.Current.Logger, 
                LogType.Info, 
                ProfilerResolver.HasCurrent ? ProfilerResolver.Current.Profiler : null,
                loggerType, 
                startMessage(), 
                completeMessage());
        }

        /// <summary>
        /// Adds a start and end log entry as Info and tracks how long it takes until disposed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startMessage"></param>
        /// <param name="completeMessage"></param>
        /// <returns></returns>
        [Obsolete("Use the Umbraco.Core.Logging.ProfilingLogger to create instances of DisposableTimer")]
        public static DisposableTimer TraceDuration<T>(string startMessage, string completeMessage)
        {
            return TraceDuration(typeof(T), startMessage, completeMessage);
        }

        /// <summary>
        /// Adds a start and end log entry as Info and tracks how long it takes until disposed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startMessage"></param>
        /// <returns></returns>
        [Obsolete("Use the Umbraco.Core.Logging.ProfilingLogger to create instances of DisposableTimer")]
        public static DisposableTimer TraceDuration<T>(string startMessage)
        {
            return TraceDuration(typeof(T), startMessage, "Complete");
        }

        /// <summary>
        /// Adds a start and end log entry as Info and tracks how long it takes until disposed.
        /// </summary>
        /// <param name="loggerType"></param>
        /// <param name="startMessage"></param>
        /// <param name="completeMessage"></param>
        /// <returns></returns>
        [Obsolete("Use the Umbraco.Core.Logging.ProfilingLogger to create instances of DisposableTimer")]
        public static DisposableTimer TraceDuration(Type loggerType, string startMessage, string completeMessage)
        {
            return new DisposableTimer(
                LoggerResolver.Current.Logger,
                LogType.Info, 
                ProfilerResolver.HasCurrent ? ProfilerResolver.Current.Profiler : null,
                loggerType,
                startMessage,
                completeMessage);
        }

        #endregion

        #region DebugDuration
        /// <summary>
        /// Adds a start and end log entry as Debug and tracks how long it takes until disposed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startMessage"></param>
        /// <param name="completeMessage"></param>
        /// <returns></returns>
        [Obsolete("Use the Umbraco.Core.Logging.ProfilingLogger to create instances of DisposableTimer")]
        public static DisposableTimer DebugDuration<T>(string startMessage, string completeMessage)
        {
            return DebugDuration(typeof(T), startMessage, completeMessage);
        }

        /// <summary>
        /// Adds a start and end log entry as Debug and tracks how long it takes until disposed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startMessage"></param>
        /// <returns></returns>
        [Obsolete("Use the Umbraco.Core.Logging.ProfilingLogger to create instances of DisposableTimer")]
        public static DisposableTimer DebugDuration<T>(string startMessage)
        {
            return DebugDuration(typeof(T), startMessage, "Complete");
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use the other methods that specify strings instead of Func")]
        public static DisposableTimer DebugDuration<T>(Func<string> startMessage, Func<string> completeMessage)
        {
            return DebugDuration(typeof(T), startMessage, completeMessage);
        }

        /// <summary>
        /// Adds a start and end log entry as Debug and tracks how long it takes until disposed.
        /// </summary>
        /// <param name="loggerType"></param>
        /// <param name="startMessage"></param>
        /// <param name="completeMessage"></param>
        /// <returns></returns>
        [Obsolete("Use the Umbraco.Core.Logging.ProfilingLogger to create instances of DisposableTimer")]
        public static DisposableTimer DebugDuration(Type loggerType, string startMessage, string completeMessage)
        {
            return new DisposableTimer(
                LoggerResolver.Current.Logger,
                LogType.Debug,
                ProfilerResolver.HasCurrent ? ProfilerResolver.Current.Profiler : null,
                loggerType,
                startMessage,
                completeMessage);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use the other methods that specify strings instead of Func")]
        public static DisposableTimer DebugDuration(Type loggerType, Func<string> startMessage, Func<string> completeMessage)
        {
            return new DisposableTimer(
                LoggerResolver.Current.Logger,
                LogType.Debug,
                ProfilerResolver.HasCurrent ? ProfilerResolver.Current.Profiler : null,
                loggerType,
                startMessage(),
                completeMessage());
        }
        #endregion

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObjectSlim"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
		{
            if (_profiler != null)
            {
                _profiler.DisposeIfDisposable();
            }

		    if (_profilerStep != null)
		    {
                _profilerStep.Dispose();
            }

		    if (_logType.HasValue && _endMessage.IsNullOrWhiteSpace() == false && _loggerType != null && _logger != null)
		    {
                switch (_logType)
                {
                    case LogType.Debug:
                        _logger.Debug(_loggerType, () => _endMessage + " (took " + Stopwatch.ElapsedMilliseconds + "ms)");
                        break;
                    case LogType.Info:
                        _logger.Info(_loggerType, () => _endMessage + " (took " + Stopwatch.ElapsedMilliseconds + "ms)");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("logType");
                }
            }

		    if (_callback != null)
		    {
                _callback.Invoke(Stopwatch.ElapsedMilliseconds);
            }
            
		}

	}
}