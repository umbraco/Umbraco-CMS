﻿using System;
using System.Linq;
using System.Threading;
using System.Web;
using log4net;

namespace Umbraco.Core.Services
{
    ///<summary>
	/// Used for logging
	///</summary>
    public class LoggingService : ILoggingService
	{
		///<summary>
		/// Returns a logger for the type specified
		///</summary>
		///<typeparam name="T"></typeparam>
		///<returns></returns>
		internal ILog LoggerFor<T>()
		{
			return LogManager.GetLogger(typeof(T));
		}

		/// <summary>
		/// Returns a logger for the object's type
		/// </summary>
		/// <param name="getTypeFromInstance"></param>
		/// <returns></returns>
		internal ILog LoggerFor(object getTypeFromInstance)
		{
			if (getTypeFromInstance == null) throw new ArgumentNullException("getTypeFromInstance");
			
			return LogManager.GetLogger(getTypeFromInstance.GetType());
		}

		/// <summary>
		/// Useful if the logger itself is running on another thread
		/// </summary>
		/// <param name="generateMessageFormat"></param>
		/// <returns></returns>
		private string PrefixThreadId(string generateMessageFormat)
		{
			return "[Thread " + Thread.CurrentThread.ManagedThreadId + "] " + generateMessageFormat;
		}


		/// <summary>
		/// Adds an error log
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="message"></param>
		/// <param name="exception"></param>
		public void Error<T>(string message, Exception exception)
		{
			Error(typeof (T), message, exception);
		}

		public void Error(Type callingType, string message, Exception exception)
		{
			var logger = LogManager.GetLogger(callingType);
			if (logger != null)
				logger.Error(PrefixThreadId(message), exception);
		}



		public void Warn(Type callingType, string message, params Func<object>[] formatItems)
		{
			var logger = LogManager.GetLogger(callingType);
			if (logger == null || !logger.IsWarnEnabled) return;
			logger.WarnFormat(PrefixThreadId(message), formatItems.Select(x => x.Invoke()).ToArray());
		}

		public void Warn(Type callingType, string message, bool showHttpTrace, params Func<object>[] formatItems)
		{
			Mandate.ParameterNotNull(callingType, "callingType");
			Mandate.ParameterNotNullOrEmpty(message, "message");

			if (showHttpTrace && HttpContext.Current != null)
			{
				HttpContext.Current.Trace.Warn(callingType.Name, string.Format(message, formatItems.Select(x => x.Invoke()).ToArray()));
			}	

			var logger = LogManager.GetLogger(callingType);
			if (logger == null || !logger.IsWarnEnabled) return;
			logger.WarnFormat(PrefixThreadId(message), formatItems.Select(x => x.Invoke()).ToArray());

		}

		public void WarnWithException(Type callingType, string message, Exception e, params Func<object>[] formatItems)
		{
			WarnWithException(callingType, message, false, e, formatItems);
		}

		public void WarnWithException(Type callingType, string message, bool showHttpTrace, Exception e, params Func<object>[] formatItems)
		{
			Mandate.ParameterNotNull(e, "e");
			Mandate.ParameterNotNull(callingType, "callingType");
			Mandate.ParameterNotNullOrEmpty(message, "message");

			if (showHttpTrace && HttpContext.Current != null)
			{
				HttpContext.Current.Trace.Warn(
					callingType.Name,
					string.Format(message, formatItems.Select(x => x.Invoke()).ToArray()), 
					e);
			}

			var logger = LogManager.GetLogger(callingType);
			if (logger == null || !logger.IsWarnEnabled) return;
			var executedParams = formatItems.Select(x => x.Invoke()).ToArray();
			logger.WarnFormat(PrefixThreadId(message) + ". Exception: " + e, executedParams);				
		} 

		/// <summary>
		/// Adds a warn log
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="message"></param>
		/// <param name="formatItems"></param>
		public void Warn<T>(string message, params Func<object>[] formatItems)
		{
			Warn(typeof(T), message, formatItems);
		}

		public void Warn<T>(string message, bool showHttpTrace, params Func<object>[] formatItems)
		{
			Warn(typeof(T), message, showHttpTrace, formatItems);
		}

		public void WarnWithException<T>(string message, Exception e, params Func<object>[] formatItems)
		{
			WarnWithException(typeof(T), message, e, formatItems);
		}
		public void WarnWithException<T>(string message, bool showHttpTrace, Exception e, params Func<object>[] formatItems)
		{
			WarnWithException(typeof(T), message, showHttpTrace, e, formatItems);
		} 

		


		/// <summary>
		/// Traces a message, only generating the message if tracing is actually enabled. Use this method to avoid calling any long-running methods such as "ToDebugString" if logging is disabled.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="generateMessage">The delegate to generate a message.</param>
		/// <remarks></remarks>
		public void Info<T>(Func<string> generateMessage)
		{
			Info(typeof(T), generateMessage);
		}

		/// <summary>
		/// Traces if tracing is enabled.
		/// </summary>
		/// <param name="callingType"></param>
		/// <param name="generateMessage"></param>
		public void Info(Type callingType, Func<string> generateMessage)
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
		public void Info(Type type, string generateMessageFormat, params Func<object>[] formatItems)
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
		public void Info<T>(string generateMessageFormat, params Func<object>[] formatItems)
		{
			Info(typeof(T), generateMessageFormat, formatItems);
		} 
	



		/// <summary>
		/// Debugs a message, only generating the message if tracing is actually enabled. Use this method to avoid calling any long-running methods such as "ToDebugString" if logging is disabled.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="generateMessage">The delegate to generate a message.</param>
		/// <remarks></remarks>
		public void Debug<T>(Func<string> generateMessage)
		{
			Debug(typeof(T), generateMessage);
		}

		/// <summary>
		/// Debugs if tracing is enabled.
		/// </summary>
		/// <param name="callingType"></param>
		/// <param name="generateMessage"></param>
		public void Debug(Type callingType, Func<string> generateMessage)
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
		public void Debug(Type type, string generateMessageFormat, params Func<object>[] formatItems)
		{
			var logger = LogManager.GetLogger(type);
			if (logger == null || !logger.IsDebugEnabled) return;
			var executedParams = formatItems.Select(x => x.Invoke()).ToArray();
			logger.DebugFormat(PrefixThreadId(generateMessageFormat), executedParams);
		}

		/// <summary>
		/// Debugs a message, only generating the message if debug is actually enabled. Use this method to avoid calling any long-running methods such as "ToDebugString" if logging is disabled.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="generateMessageFormat">The generate message format.</param>
		/// <param name="formatItems">The format items.</param>
		/// <remarks></remarks>
		public void Debug<T>(string generateMessageFormat, params Func<object>[] formatItems)
		{
			Debug(typeof(T), generateMessageFormat, formatItems);
		}

		/// <summary>
		/// Debugs a message and also writes to the TraceContext specified, useful for when you would like the debug
		/// output also displayed in the Http trace output.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="generateMessageFormat"></param>
		/// <param name="showHttpTrace"></param>
		/// <param name="formatItems"></param>
		public void Debug<T>(string generateMessageFormat, bool showHttpTrace, params Func<object>[] formatItems)
		{
			if (showHttpTrace && HttpContext.Current != null)
			{
				HttpContext.Current.Trace.Write(
					typeof(T).Name,
					string.Format(generateMessageFormat, formatItems.Select(x => x()).ToArray()));	
			}			
			Debug(typeof(T), generateMessageFormat, formatItems);
		}

	}
}
