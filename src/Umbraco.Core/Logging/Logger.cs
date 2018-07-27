using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core.Diagnostics;
using Serilog;

namespace Umbraco.Core.Logging
{
    ///<summary>
    /// Implements <see cref="ILogger"/> on top of log4net.
    ///</summary>
    public class Logger : ILogger
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="Logger"/> class with a log4net configuration file.
        /// </summary>
        /// <param name="log4NetConfigFile"></param>
        public Logger(FileInfo log4NetConfigFile)
            : this()
        {
            //XmlConfigurator.Configure(log4NetConfigFile);
        }

        // private for CreateWithDefaultConfiguration
        private Logger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug() //Set to highest level of logging (as any sinks may want to restrict it to Errors only)
                .Enrich.WithProcessId()
                .Enrich.WithProcessName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("AppDomainId", AppDomain.CurrentDomain.Id)
                .Enrich.WithProperty("AppDomainAppId", HttpRuntime.AppDomainAppId.ReplaceNonAlphanumericChars(string.Empty))
                .WriteTo.File(AppDomain.CurrentDomain.BaseDirectory + "\\logs\\warren.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        /// <summary>
        /// Creates a logger with the default log4net configuration discovered (i.e. from the web.config).
        /// </summary>
        /// <remarks>Used by UmbracoApplicationBase to get its logger.</remarks>
        public static Logger CreateWithDefaultConfiguration()
        {
            return new Logger();
        }

        /// <inheritdoc/>
        public void Error(Type reporting, string message, Exception exception = null)
        {
            var logger = Log.Logger;
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

        /// <inheritdoc/>
        public void Warn(Type reporting, string format)
        {
            var logger = Log.Logger;
            logger?.Warning(format);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, Func<string> messageBuilder)
        {
            var logger = Log.Logger;
            logger?.Warning(messageBuilder());
        }

        /// <inheritdoc/>
        //public void Warn(Type reporting, string format, params object[] args)
        //{
        //    var logger = Log.Logger;
        //    if (logger == null) return;
        //    logger.WarnFormat(format, args);
        //}

        ///// <inheritdoc/>
        //public void Warn(Type reporting, string format, params Func<object>[] args)
        //{
        //    var logger = LogManager.GetLogger(reporting);
        //    if (logger == null || logger.IsWarnEnabled == false) return;
        //    logger.WarnFormat(format, args.Select(x => x.Invoke()).ToArray());
        //}

        /// <inheritdoc/>
        public void Warn(Type reporting, Exception exception, string message)
        {
            var logger = Log.Logger;
            logger?.Warning(message, exception);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, Exception exception, Func<string> messageBuilder)
        {
            var logger = Log.Logger;
            logger?.Warning(messageBuilder(), exception);
        }

        ///// <inheritdoc/>
        //public void Warn(Type reporting, Exception exception, string format, params object[] args)
        //{
        //    var logger = LogManager.GetLogger(reporting);
        //    if (logger == null || logger.IsWarnEnabled == false) return;
        //    // there is no WarnFormat overload that accepts an exception
        //    // format the message the way log4net would do it (see source code LogImpl.cs)
        //    var message = new SystemStringFormat(CultureInfo.InvariantCulture, format, args);
        //    logger.Warn(message, exception);
        //}

        ///// <inheritdoc/>
        //public void Warn(Type reporting, Exception exception, string format, params Func<object>[] args)
        //{
        //    var logger = LogManager.GetLogger(reporting);
        //    if (logger == null || logger.IsWarnEnabled == false) return;
        //    // there is no WarnFormat overload that accepts an exception
        //    // format the message the way log4net would do it (see source code LogImpl.cs)
        //    var message = new SystemStringFormat(CultureInfo.InvariantCulture, format, args.Select(x => x.Invoke()).ToArray());
        //    logger.Warn(message, exception);
        //}

        /// <inheritdoc/>
        public void Info(Type reporting, string message)
        {
            var logger = Log.Logger;
            logger?.Information(message);
        }

        /// <inheritdoc/>
        public void Info(Type reporting, Func<string> generateMessage)
        {
            var logger = Log.Logger;
            logger?.Information(generateMessage());
        }

        ///// <inheritdoc/>
        //public void Info(Type reporting, string format, params object[] args)
        //{
        //    var logger = LogManager.GetLogger(reporting);
        //    if (logger == null || logger.IsInfoEnabled == false) return;
        //    logger.InfoFormat(format, args);
        //}

        ///// <inheritdoc/>
        //public void Info(Type reporting, string format, params Func<object>[] args)
        //{
        //    var logger = LogManager.GetLogger(reporting);
        //    if (logger == null || logger.IsInfoEnabled == false) return;
        //    logger.InfoFormat(format, args.Select(x => x.Invoke()).ToArray());
        //}

        /// <inheritdoc/>
        public void Debug(Type reporting, string message)
        {
            var logger = Log.Logger;
            logger?.Debug(message);
        }

        /// <inheritdoc/>
        public void Debug(Type reporting, Func<string> messageBuilder)
        {
            var logger = Log.Logger;
            logger?.Debug(messageBuilder());
        }

        ///// <inheritdoc/>
        //public void Debug(Type reporting, string format, params object[] args)
        //{
        //    var logger = LogManager.GetLogger(reporting);
        //    if (logger == null || logger.IsDebugEnabled == false) return;
        //    logger.DebugFormat(format, args);
        //}

        ///// <inheritdoc/>
        //public void Debug(Type reporting, string format, params Func<object>[] args)
        //{
        //    var logger = LogManager.GetLogger(reporting);
        //    if (logger == null || logger.IsDebugEnabled == false) return;
        //    logger.DebugFormat(format, args.Select(x => x.Invoke()).ToArray());
        //}
    }
}
