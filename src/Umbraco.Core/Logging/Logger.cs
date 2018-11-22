using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using log4net;
using log4net.Config;
using Umbraco.Core.Configuration;
using Umbraco.Core.Diagnostics;

namespace Umbraco.Core.Logging
{
    ///<summary>
	/// Used for logging
	///</summary>
    public class Logger : ILogger
    {

        public Logger(FileInfo log4NetConfigFile)
            :this()
        {
            XmlConfigurator.Configure(log4NetConfigFile);
        }

        private Logger()
        {
            //Add custom global properties to the log4net context that we can use in our logging output

            log4net.GlobalContext.Properties["processId"] = Process.GetCurrentProcess().Id;
            log4net.GlobalContext.Properties["appDomainId"] = AppDomain.CurrentDomain.Id;
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

		public void Error(Type callingType, string message, Exception exception)
		{
            var logger = LogManager.GetLogger(callingType);
		    if (logger == null) return;

		    var dump = false;

		    if (IsTimeoutThreadAbortException(exception))
		    {
		        message += "\r\nThe thread has been aborted, because the request has timed out.";

                // dump if configured, or if stacktrace contains Monitor.ReliableEnter
		        dump = UmbracoConfig.For.CoreDebug().DumpOnTimeoutThreadAbort || IsMonitorEnterThreadAbortException(exception);

                // dump if it is ok to dump (might have a cap on number of dump...)
		        dump &= MiniDump.OkToDump();
		    }

            if (dump)
		    {
                try
                {
                    var dumped = MiniDump.Dump(withException: true);
                    message += dumped
                        ? "\r\nA minidump was created in App_Data/MiniDump"
                        : "\r\nFailed to create a minidump";
                }
                catch (Exception e)
                {
                    message += string.Format("\r\nFailed to create a minidump ({0}: {1})", e.GetType().FullName, e.Message);
                }
            }

            logger.Error(message, exception);
		}

        private static bool IsMonitorEnterThreadAbortException(Exception exception)
        {
            var abort = exception as ThreadAbortException;
            if (abort == null) return false;

            var stacktrace = abort.StackTrace;
            return stacktrace.Contains("System.Threading.Monitor.ReliableEnter");
        }

        private static bool IsTimeoutThreadAbortException(Exception exception)
        {
            var abort = exception as ThreadAbortException;
            if (abort == null) return false;

            if (abort.ExceptionState == null) return false;

            var stateType = abort.ExceptionState.GetType();
            if (stateType.FullName != "System.Web.HttpApplication+CancelModuleException") return false;

            var timeoutField = stateType.GetField("_timeout", BindingFlags.Instance | BindingFlags.NonPublic);
            if (timeoutField == null) return false;

            return (bool) timeoutField.GetValue(abort.ExceptionState);
        }

        public void Warn(Type callingType, string message, params Func<object>[] formatItems)
		{
			var logger = LogManager.GetLogger(callingType);
			if (logger == null || logger.IsWarnEnabled == false) return;
			logger.WarnFormat((message), formatItems.Select(x => x.Invoke()).ToArray());
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
			logger.WarnFormat((message), formatItems.Select(x => x.Invoke()).ToArray());

		}

		public void WarnWithException(Type callingType, string message, Exception e, params Func<object>[] formatItems)
		{
            Mandate.ParameterNotNull(e, "e");
            Mandate.ParameterNotNull(callingType, "callingType");
            Mandate.ParameterNotNullOrEmpty(message, "message");

            var logger = LogManager.GetLogger(callingType);
            if (logger == null || logger.IsWarnEnabled == false) return;
            var executedParams = formatItems.Select(x => x.Invoke()).ToArray();
            logger.WarnFormat((message) + ". Exception: " + e, executedParams);
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
			logger.Info((generateMessage.Invoke()));
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
			logger.InfoFormat((generateMessageFormat), executedParams);
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
			logger.Debug((generateMessage.Invoke()));
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
			logger.DebugFormat((generateMessageFormat), executedParams);
		}


	}
}
