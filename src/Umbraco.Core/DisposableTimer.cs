using System;
using System.Diagnostics;
using System.Web;
using Umbraco.Core.Logging;
using Umbraco.Core.Profiling;

namespace Umbraco.Core
{
	/// <summary>
	/// Starts the timer and invokes a  callback upon disposal. Provides a simple way of timing an operation by wrapping it in a <code>using</code> (C#) statement.
	/// </summary>
	/// <example>
	/// <code>
	/// 
	/// using (DisposableTimer.TraceDuration{MyType}("starting", "finished"))
	/// {
    ///     Thread.Sleep(567);
	/// }
	/// 
	/// Console.WriteLine("Testing Stopwatchdisposable, should be 567:");
	/// using (var timer = new DisposableTimer(result => Console.WriteLine("Took {0}ms", result)))
	/// {
	///     Thread.Sleep(567);
	/// }
	/// </code>
	/// </example>
	public class DisposableTimer : DisposableObject
	{
		private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
		private readonly Action<long> _callback;

		protected DisposableTimer(Action<long> callback)
		{
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
        public static DisposableTimer TraceDuration<T>(Func<string> startMessage, Func<string> completeMessage)
        {
            return TraceDuration(typeof(T), startMessage, completeMessage);
        }

        public static DisposableTimer TraceDuration(Type loggerType, Func<string> startMessage, Func<string> completeMessage)
        {
            var startMsg = startMessage();
            LogHelper.Info(loggerType, startMsg);
            if (HttpContext.Current != null)
                HttpContext.Current.Trace.Write("Start: " + startMsg);
            var profiler = ActivateProfiler(loggerType, startMsg);
            return new DisposableTimer(x =>
                {
                    profiler.DisposeIfDisposable();
                    LogHelper.Info(loggerType, () => completeMessage() + " (took " + x + "ms)");
                    if (HttpContext.Current != null)
                        HttpContext.Current.Trace.Write("End: " + startMsg);
                });
        }

        /// <summary>
        /// Adds a start and end log entry as Info and tracks how long it takes until disposed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startMessage"></param>
        /// <param name="completeMessage"></param>
        /// <returns></returns>
        public static DisposableTimer TraceDuration<T>(string startMessage, string completeMessage)
        {
            return TraceDuration(typeof(T), startMessage, completeMessage);
        }

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
        public static DisposableTimer TraceDuration(Type loggerType, string startMessage, string completeMessage)
        {
            LogHelper.Info(loggerType, startMessage);
            if (HttpContext.Current != null)
                HttpContext.Current.Trace.Write("Start: " + startMessage);
            var profiler = ActivateProfiler(loggerType, startMessage);
            return new DisposableTimer(x =>
                {
                    profiler.DisposeIfDisposable();
                    LogHelper.Info(loggerType, () => completeMessage + " (took " + x + "ms)");
                    if (HttpContext.Current != null)
                        HttpContext.Current.Trace.Write("End: " + startMessage);
                });
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
        public static DisposableTimer DebugDuration<T>(string startMessage, string completeMessage)
        {
            return DebugDuration(typeof(T), startMessage, completeMessage);
        }

        public static DisposableTimer DebugDuration<T>(string startMessage)
        {
            return DebugDuration(typeof(T), startMessage, "Complete");
        }

        /// <summary>
        /// Adds a start and end log entry as Debug and tracks how long it takes until disposed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startMessage"></param>
        /// <param name="completeMessage"></param>
        /// <returns></returns>
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
        public static DisposableTimer DebugDuration(Type loggerType, string startMessage, string completeMessage)
        {
            LogHelper.Debug(loggerType, startMessage);
            if (HttpContext.Current != null)
                HttpContext.Current.Trace.Write("Start: " + startMessage);
            var profiler = ActivateProfiler(loggerType, startMessage);
            return new DisposableTimer(x =>
                {
                    profiler.DisposeIfDisposable();
                    LogHelper.Debug(loggerType, () => completeMessage + " (took " + x + "ms)");
                    if (HttpContext.Current != null)
                        HttpContext.Current.Trace.Write("End: " + startMessage);
                });
        }

        /// <summary>
        /// Adds a start and end log entry as Debug and tracks how long it takes until disposed.
        /// </summary>
        /// <param name="loggerType"></param>
        /// <param name="startMessage"></param>
        /// <param name="completeMessage"></param>
        /// <returns></returns>
        public static DisposableTimer DebugDuration(Type loggerType, Func<string> startMessage, Func<string> completeMessage)
        {
            var msg = startMessage();
            LogHelper.Debug(loggerType, msg);
            if (HttpContext.Current != null)
                HttpContext.Current.Trace.Write("Start: " + startMessage);
            var profiler = ActivateProfiler(loggerType, msg);
            return new DisposableTimer(x =>
                {
                    profiler.DisposeIfDisposable();
                    LogHelper.Debug(loggerType, () => completeMessage() + " (took " + x + "ms)");
                    if (HttpContext.Current != null)
                        HttpContext.Current.Trace.Write("End: " + startMessage);
                });
        } 
        #endregion

		/// <summary>
		/// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
		/// </summary>
		protected override void DisposeResources()
		{
			_callback.Invoke(Stopwatch.ElapsedMilliseconds);
		}

        private static IDisposable ActivateProfiler(Type loggerType, string profileName)
        {
            try
            {
                return ProfilerResolver.Current.Profiler.Step(loggerType, profileName);
            }
            catch (InvalidOperationException)
            {
                //swallow this exception, it will occur if the ProfilerResolver is not initialized... generally only in 
                // unit tests.
            }
            return null;
        }
	}
}