using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;

namespace Umbraco.Core.Logging
{
	///<summary>
	/// Used for logging
	///</summary>
	internal static class LogHelper
	{
		///<summary>
		/// Returns a logger for the type specified
		///</summary>
		///<typeparam name="T"></typeparam>
		///<returns></returns>
		public static ILog LoggerFor<T>()
		{
			return LogManager.GetLogger(typeof(T));
		}

		/// <summary>
		/// Returns a logger for the object's type
		/// </summary>
		/// <param name="getTypeFromInstance"></param>
		/// <returns></returns>
		public static ILog LoggerFor(object getTypeFromInstance)
		{
			if (getTypeFromInstance == null) throw new ArgumentNullException("getTypeFromInstance");
			
			return LogManager.GetLogger(getTypeFromInstance.GetType());
		}

		/// <summary>
		/// Useful if the logger itself is running on another thread
		/// </summary>
		/// <param name="generateMessageFormat"></param>
		/// <returns></returns>
		private static string PrefixThreadId(string generateMessageFormat)
		{
			return "[Thread " + Thread.CurrentThread.ManagedThreadId + "] " + generateMessageFormat;
		}

		#region Error
		/// <summary>
		/// Adds an error log
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="message"></param>
		/// <param name="exception"></param>
		public static void Error<T>(string message, Exception exception)
		{
			var logger = LoggerFor<T>();
			if (logger != null)
				logger.Error(PrefixThreadId(message), exception);
		}
		#endregion

		#region Warn
		public static void Warn(Type callingType, string message)
		{
			var logger = LogManager.GetLogger(callingType);
			if (logger != null)
				logger.Warn(PrefixThreadId(message));
		}

		public static void Warn(Type callingType, string message, params object[] format)
		{
			var logger = LogManager.GetLogger(callingType);
			if (logger != null)
				logger.WarnFormat(PrefixThreadId(message), format);
		}

		/// <summary>
		/// Adds a warn log
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="message"></param>
		public static void Warn<T>(string message)
		{
			var logger = LoggerFor<T>();
			if (logger != null)
				logger.Warn(PrefixThreadId(message));
		}

		/// <summary>
		/// Adds a warn log
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="format"></param>
		/// <param name="items"></param>
		public static void Warn<T>(string format, params object[] items)
		{
			var logger = LoggerFor<T>();
			if (logger != null)
				logger.WarnFormat(PrefixThreadId(format), items);
		} 
		#endregion

		#region Info
		/// <summary>
		/// Traces a message, only generating the message if tracing is actually enabled. Use this method to avoid calling any long-running methods such as "ToDebugString" if logging is disabled.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="generateMessage">The delegate to generate a message.</param>
		/// <remarks></remarks>
		public static void Info<T>(Func<string> generateMessage)
		{
			Info(typeof(T), generateMessage);
		}

		/// <summary>
		/// Traces if tracing is enabled.
		/// </summary>
		/// <param name="callingType"></param>
		/// <param name="generateMessage"></param>
		public static void Info(Type callingType, Func<string> generateMessage)
		{
			var logger = LogManager.GetLogger(callingType);
			if (logger == null || !logger.IsInfoEnabled) return;
			logger.Info(PrefixThreadId(generateMessage.Invoke()));
		}

		/// <summary>
		/// Traces if tracing is enabled.
		/// </summary>
		/// <param name="type">The type for the logging namespace.</param>
		/// <param name="generateMessageFormat">The message format.</param>
		/// <param name="formatItems">The format items.</param>
		public static void Info(Type type, string generateMessageFormat, params Func<object>[] formatItems)
		{
			var logger = LogManager.GetLogger(type);
			if (logger == null || !logger.IsInfoEnabled) return;
			var executedParams = formatItems.Select(x => x.Invoke()).ToArray();
			logger.InfoFormat(PrefixThreadId(generateMessageFormat), executedParams);
		}

		/// <summary>
		/// Traces a message, only generating the message if tracing is actually enabled. Use this method to avoid calling any long-running methods such as "ToDebugString" if logging is disabled.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="generateMessageFormat">The generate message format.</param>
		/// <param name="formatItems">The format items.</param>
		/// <remarks></remarks>
		public static void Info<T>(string generateMessageFormat, params Func<object>[] formatItems)
		{
			Info(typeof(T), generateMessageFormat, formatItems);
		} 
		#endregion

		#region Debug
		/// <summary>
		/// Debugs a message, only generating the message if tracing is actually enabled. Use this method to avoid calling any long-running methods such as "ToDebugString" if logging is disabled.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="generateMessage">The delegate to generate a message.</param>
		/// <remarks></remarks>
		public static void Debug<T>(Func<string> generateMessage)
		{
			Debug(typeof(T), generateMessage);
		}

		/// <summary>
		/// Debugs if tracing is enabled.
		/// </summary>
		/// <param name="callingType"></param>
		/// <param name="generateMessage"></param>
		public static void Debug(Type callingType, Func<string> generateMessage)
		{
			var logger = LogManager.GetLogger(callingType);
			if (logger == null || !logger.IsDebugEnabled) return;
			logger.Debug(PrefixThreadId(generateMessage.Invoke()));
		}

		/// <summary>
		/// Debugs if tracing is enabled.
		/// </summary>
		/// <param name="type">The type for the logging namespace.</param>
		/// <param name="generateMessageFormat">The message format.</param>
		/// <param name="formatItems">The format items.</param>
		public static void Debug(Type type, string generateMessageFormat, params Func<object>[] formatItems)
		{
			var logger = LogManager.GetLogger(type);
			if (logger == null || !logger.IsDebugEnabled) return;
			var executedParams = formatItems.Select(x => x.Invoke()).ToArray();
			logger.DebugFormat(PrefixThreadId(generateMessageFormat), executedParams);
		}

		/// <summary>
		/// Debugs a message, only generating the message if tracing is actually enabled. Use this method to avoid calling any long-running methods such as "ToDebugString" if logging is disabled.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="generateMessageFormat">The generate message format.</param>
		/// <param name="formatItems">The format items.</param>
		/// <remarks></remarks>
		public static void Debug<T>(string generateMessageFormat, params Func<object>[] formatItems)
		{
			Debug(typeof(T), generateMessageFormat, formatItems);
		}
		#endregion
		
	}
}
