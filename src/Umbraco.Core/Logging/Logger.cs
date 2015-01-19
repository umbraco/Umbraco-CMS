using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using log4net;
using log4net.Config;

namespace Umbraco.Core.Logging
{
    ///<summary>
	/// Used for logging
	///</summary>
    public class Logger : ILogger
	{
        public Logger(FileInfo log4NetConfigFile)
        {
            XmlConfigurator.Configure(log4NetConfigFile);
        }

        private Logger()
        {
            
        }

        /// <summary>
        /// Creates a logger with the default log4net configuration discovered (i.e. from the web.config)
        /// </summary>
        /// <returns></returns>
        public static Logger CreateWithDefaultLog4NetConfiguration()
        {
            return new Logger();
        }

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

		public void Error(Type callingType, string message, Exception exception)
		{
			var logger = LogManager.GetLogger(callingType);
			if (logger != null)
				logger.Error(PrefixThreadId(message), exception);
		}



		public void Warn(Type callingType, string message, params Func<object>[] formatItems)
		{
			var logger = LogManager.GetLogger(callingType);
			if (logger == null || logger.IsWarnEnabled == false) return;
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
			if (logger == null || logger.IsWarnEnabled == false) return;
			logger.WarnFormat(PrefixThreadId(message), formatItems.Select(x => x.Invoke()).ToArray());

		}

		public void WarnWithException(Type callingType, string message, Exception e, params Func<object>[] formatItems)
		{
            Mandate.ParameterNotNull(e, "e");
            Mandate.ParameterNotNull(callingType, "callingType");
            Mandate.ParameterNotNullOrEmpty(message, "message");

            var logger = LogManager.GetLogger(callingType);
            if (logger == null || logger.IsWarnEnabled == false) return;
            var executedParams = formatItems.Select(x => x.Invoke()).ToArray();
            logger.WarnFormat(PrefixThreadId(message) + ". Exception: " + e, executedParams);		
		}

		/// <summary>
		/// Traces if tracing is enabled.
		/// </summary>
		/// <param name="callingType"></param>
		/// <param name="generateMessage"></param>
		public void Info(Type callingType, Func<string> generateMessage)
		{
			var logger = LogManager.GetLogger(callingType);
			if (logger == null || logger.IsInfoEnabled == false) return;
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
			if (logger == null || logger.IsInfoEnabled == false) return;
			var executedParams = formatItems.Select(x => x.Invoke()).ToArray();
			logger.InfoFormat(PrefixThreadId(generateMessageFormat), executedParams);
		}


		/// <summary>
		/// Debugs if tracing is enabled.
		/// </summary>
		/// <param name="callingType"></param>
		/// <param name="generateMessage"></param>
		public void Debug(Type callingType, Func<string> generateMessage)
		{
			var logger = LogManager.GetLogger(callingType);
			if (logger == null || logger.IsDebugEnabled == false) return;
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
			if (logger == null || logger.IsDebugEnabled == false) return;
			var executedParams = formatItems.Select(x => x.Invoke()).ToArray();
			logger.DebugFormat(PrefixThreadId(generateMessageFormat), executedParams);
		}


	}
}
