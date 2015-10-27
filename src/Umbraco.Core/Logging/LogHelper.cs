using System;
using System.Linq;
using System.Threading;
using System.Web;
using log4net;

namespace Umbraco.Core.Logging
{
    ///<summary>
	/// Used for logging, ILogger should be used instead but this is available for static access to logging
	///</summary>
	/// <remarks>
    /// this wraps ILogger 
	/// </remarks>
	public static class LogHelper
	{
		#region Error
		/// <summary>
		/// Adds an error log
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="message"></param>
		/// <param name="exception"></param>
		public static void Error<T>(string message, Exception exception)
		{
		    if (LoggerResolver.HasCurrent == false || LoggerResolver.Current.HasValue == false) return;
		    LoggerResolver.Current.Logger.Error<T>(message, exception);
		}

		public static void Error(Type callingType, string message, Exception exception)
		{
            if (LoggerResolver.HasCurrent == false || LoggerResolver.Current.HasValue == false) return;
            LoggerResolver.Current.Logger.Error(callingType, message, exception);
		}

		#endregion

		#region Warn		

		public static void Warn(Type callingType, string message, params Func<object>[] formatItems)
		{
            if (LoggerResolver.HasCurrent == false || LoggerResolver.Current.HasValue == false) return;
            LoggerResolver.Current.Logger.Warn(callingType, message, formatItems);
		}

        [Obsolete("Warnings with http trace should not be used. This method will be removed in future versions")]
		public static void Warn(Type callingType, string message, bool showHttpTrace, params Func<object>[] formatItems)
		{
			Mandate.ParameterNotNull(callingType, "callingType");
			Mandate.ParameterNotNullOrEmpty(message, "message");

			if (showHttpTrace && HttpContext.Current != null)
			{
				HttpContext.Current.Trace.Warn(callingType.Name, string.Format(message, formatItems.Select(x => x.Invoke()).ToArray()));
			}

            if (LoggerResolver.HasCurrent == false || LoggerResolver.Current.HasValue == false) return;
            LoggerResolver.Current.Logger.Warn(callingType, message, formatItems);

		}

		public static void WarnWithException(Type callingType, string message, Exception e, params Func<object>[] formatItems)
		{
			WarnWithException(callingType, message, false, e, formatItems);
		}

        [Obsolete("Warnings with http trace should not be used. This method will be removed in future versions")]
		public static void WarnWithException(Type callingType, string message, bool showHttpTrace, Exception e, params Func<object>[] formatItems)
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

            if (LoggerResolver.HasCurrent == false || LoggerResolver.Current.HasValue == false) return;
            LoggerResolver.Current.Logger.WarnWithException(callingType, message, e, formatItems);
		} 

		/// <summary>
		/// Adds a warn log
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="message"></param>
		/// <param name="formatItems"></param>
		public static void Warn<T>(string message, params Func<object>[] formatItems)
		{
			Warn(typeof(T), message, formatItems);
		}

        [Obsolete("Warnings with http trace should not be used. This method will be removed in future versions")]
		public static void Warn<T>(string message, bool showHttpTrace, params Func<object>[] formatItems)
		{
			Warn(typeof(T), message, showHttpTrace, formatItems);
		}

		public static void WarnWithException<T>(string message, Exception e, params Func<object>[] formatItems)
		{
			WarnWithException(typeof(T), message, e, formatItems);
		}

        [Obsolete("Warnings with http trace should not be used. This method will be removed in future versions")]
		public static void WarnWithException<T>(string message, bool showHttpTrace, Exception e, params Func<object>[] formatItems)
		{
			WarnWithException(typeof(T), message, showHttpTrace, e, formatItems);
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
            if (LoggerResolver.HasCurrent == false || LoggerResolver.Current.HasValue == false) return;
            LoggerResolver.Current.Logger.Info(callingType, generateMessage);
		}

		/// <summary>
		/// Traces if tracing is enabled.
		/// </summary>
		/// <param name="type">The type for the logging namespace.</param>
		/// <param name="generateMessageFormat">The message format.</param>
		/// <param name="formatItems">The format items.</param>
		public static void Info(Type type, string generateMessageFormat, params Func<object>[] formatItems)
		{
            if (LoggerResolver.HasCurrent == false || LoggerResolver.Current.HasValue == false) return;
            LoggerResolver.Current.Logger.Info(type, generateMessageFormat, formatItems);
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
            if (LoggerResolver.HasCurrent == false || LoggerResolver.Current.HasValue == false) return;
            LoggerResolver.Current.Logger.Debug(callingType, generateMessage);
		}

		/// <summary>
		/// Debugs if tracing is enabled.
		/// </summary>
		/// <param name="type">The type for the logging namespace.</param>
		/// <param name="generateMessageFormat">The message format.</param>
		/// <param name="formatItems">The format items.</param>
		public static void Debug(Type type, string generateMessageFormat, params Func<object>[] formatItems)
		{
            if (LoggerResolver.HasCurrent == false || LoggerResolver.Current.HasValue == false) return;
            LoggerResolver.Current.Logger.Debug(type, generateMessageFormat, formatItems);
		}

		/// <summary>
		/// Debugs a message, only generating the message if debug is actually enabled. Use this method to avoid calling any long-running methods such as "ToDebugString" if logging is disabled.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="generateMessageFormat">The generate message format.</param>
		/// <param name="formatItems">The format items.</param>
		/// <remarks></remarks>
		public static void Debug<T>(string generateMessageFormat, params Func<object>[] formatItems)
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
        [Obsolete("Warnings with http trace should not be used. This method will be removed in future versions")]
		public static void Debug<T>(string generateMessageFormat, bool showHttpTrace, params Func<object>[] formatItems)
		{
			if (showHttpTrace && HttpContext.Current != null)
			{
				HttpContext.Current.Trace.Write(
					typeof(T).Name,
					string.Format(generateMessageFormat, formatItems.Select(x => x()).ToArray()));	
			}			
			Debug(typeof(T), generateMessageFormat, formatItems);
		}

		#endregion
		
	}
}
