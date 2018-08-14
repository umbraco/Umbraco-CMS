using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Umbraco.Core.Configuration;
using Umbraco.Core.Diagnostics;
using Serilog;
using Umbraco.Core.Logging.SerilogExtensions;

namespace Umbraco.Core.Logging
{
    ///<summary>
    /// Implements <see cref="ILogger"/> on top of Serilog.
    ///</summary>
    public class Logger : ILogger
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="Logger"/> class with a configuration file.
        /// </summary>
        /// <param name="logConfigFile"></param>
        public Logger(FileInfo logConfigFile)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.AppSettings(filePath: AppDomain.CurrentDomain.BaseDirectory + logConfigFile)
                .CreateLogger();
        }

        public Logger(LoggerConfiguration logConfig)
        {
            //Configure Serilog static global logger with config passed in
            Log.Logger = logConfig.CreateLogger();
        }

        /// <summary>
        /// Creates a logger with some pre-definied configuration and remainder from config file
        /// </summary>
        /// <remarks>Used by UmbracoApplicationBase to get its logger.</remarks>
        public static Logger CreateWithDefaultConfiguration()
        {
            var loggerConfig = new LoggerConfiguration();
            loggerConfig
                .MinimalConfiguration()
                .OutputDefaultTextFile()
                .OutputDefaultJsonFile()
                .ReadFromConfigFile()
                .ReadFromUserConfigFile();

            return new Logger(loggerConfig);
        }

        /// <inheritdoc/>
        [Obsolete("Use the message template version")]
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

        public void Error(Type reporting, string messageTemplate, Exception exception = null, params object[] propertyValues)
        {
            var dump = false;

            if (IsTimeoutThreadAbortException(exception))
            {
                messageTemplate += "\r\nThe thread has been aborted, because the request has timed out.";

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
                    messageTemplate += dumped
                        ? "\r\nA minidump was created in App_Data/MiniDump"
                        : "\r\nFailed to create a minidump";
                }
                catch (Exception e)
                {
                    //Log a new entry (as opposed to appending to same log entry)
                    Error(e.GetType(), "Failed to create a minidump at App_Data/MiniDump ({ExType}: {ExMessage}", e, e.GetType().FullName, e.Message);
                }
            }

            var logger = Log.Logger;
            logger?.ForContext(reporting).Error(exception, messageTemplate, propertyValues);
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
        public void Warn(Type reporting, string messageTemplate, params object[] propertyValues)
        {
            var logger = Log.Logger;
            logger?.ForContext(reporting).Warning(messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, Exception exception, string message)
        {
            var logger = Log.Logger;
            logger?.ForContext(reporting).Warning(message, exception);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, Exception exception, Func<string> messageBuilder)
        {
            var logger = Log.Logger;
            logger?.ForContext(reporting).Warning(messageBuilder(), exception);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            var logger = Log.Logger;
            logger?.ForContext(reporting).Warning(exception, messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void Info(Type reporting, string message)
        {
            var logger = Log.Logger;
            logger?.ForContext(reporting).Information(message);
        }

        /// <inheritdoc/>
        public void Info(Type reporting, Func<string> generateMessage)
        {
            var logger = Log.Logger;
            logger?.ForContext(reporting).Information(generateMessage());
        }

        /// <inheritdoc/>
        public void Info(Type reporting, string messageTemplate, params object[] propertyValues)
        {
            var logger = Log.Logger;
            logger?.ForContext(reporting).Information(messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void Debug(Type reporting, string message)
        {
            var logger = Log.Logger;
            logger?.ForContext(reporting).Debug(message);
        }

        /// <inheritdoc/>
        public void Debug(Type reporting, Func<string> messageBuilder)
        {
            var logger = Log.Logger;
            logger?.ForContext(reporting).Debug(messageBuilder());
        }

        /// <inheritdoc/>
        public void Debug(Type reporting, string messageTemplate, params object[] propertyValues)
        {
            var logger = Log.Logger;
            logger?.ForContext(reporting).Debug(messageTemplate, propertyValues);
        }
    }
}
