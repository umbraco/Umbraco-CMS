using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Serilog;
using Serilog.Events;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Diagnostics;

namespace Umbraco.Core.Logging.Serilog
{
    ///<summary>
    /// Implements <see cref="ILogger"/> on top of Serilog.
    ///</summary>
    public class SerilogLogger : ILogger, IDisposable
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="SerilogLogger"/> class with a configuration file.
        /// </summary>
        /// <param name="logConfigFile"></param>
        public SerilogLogger(FileInfo logConfigFile)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.AppSettings(filePath: AppDomain.CurrentDomain.BaseDirectory + logConfigFile)
                .CreateLogger();
        }

        public SerilogLogger(LoggerConfiguration logConfig)
        {
            //Configure Serilog static global logger with config passed in
            Log.Logger = logConfig.CreateLogger();
        }

        /// <summary>
        /// Creates a logger with some pre-defined configuration and remainder from config file
        /// </summary>
        /// <remarks>Used by UmbracoApplicationBase to get its logger.</remarks>
        public static SerilogLogger CreateWithDefaultConfiguration()
        {
            var loggerConfig = new LoggerConfiguration();
            loggerConfig
                .MinimalConfiguration()
                .ReadFromConfigFile()
                .ReadFromUserConfigFile();

            return new SerilogLogger(loggerConfig);
        }

        /// <summary>
        /// Gets a contextualized logger.
        /// </summary>
        private global::Serilog.ILogger LoggerFor(Type reporting)
            => Log.Logger.ForContext(reporting);

        /// <summary>
        /// Maps Umbraco's log level to Serilog's.
        /// </summary>
        private LogEventLevel MapLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return LogEventLevel.Debug;
                case LogLevel.Error:
                    return LogEventLevel.Error;
                case LogLevel.Fatal:
                    return LogEventLevel.Fatal;
                case LogLevel.Information:
                    return LogEventLevel.Information;
                case LogLevel.Verbose:
                    return LogEventLevel.Verbose;
                case LogLevel.Warning:
                    return LogEventLevel.Warning;
            }

            throw new NotSupportedException($"LogLevel \"{level}\" is not supported.");
        }

        /// <inheritdoc/>
        public bool IsEnabled(Type reporting, LogLevel level)
            => LoggerFor(reporting).IsEnabled(MapLevel(level));

        /// <inheritdoc/>
        public void Fatal(Type reporting, Exception exception, string message)
        {
            var logger = LoggerFor(reporting);
            DumpThreadAborts(logger, LogEventLevel.Fatal, exception, ref message);
            logger.Fatal(exception, message);
        }

        /// <inheritdoc/>
        public void Fatal(Type reporting, Exception exception)
        {
            var logger = LoggerFor(reporting);
            var message = "Exception.";
            DumpThreadAborts(logger, LogEventLevel.Fatal, exception, ref message);
            logger.Fatal(exception, message);
        }

        /// <inheritdoc/>
        public void Fatal(Type reporting, string message)
        {
            LoggerFor(reporting).Fatal(message);
        }

        /// <inheritdoc/>
        public void Fatal(Type reporting, string messageTemplate, params object[] propertyValues)
        {
            LoggerFor(reporting).Fatal(messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void Fatal(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            var logger = LoggerFor(reporting);
            DumpThreadAborts(logger, LogEventLevel.Fatal, exception, ref messageTemplate);
            logger.Fatal(exception, messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void Fatal<T0>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0)
        {
            var logger = LoggerFor(reporting);
            DumpThreadAborts(logger, LogEventLevel.Fatal, exception, ref messageTemplate);
            logger.Fatal(exception, messageTemplate, propertyValue0);
        }

        /// <inheritdoc/>
        public void Fatal<T0,T1>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            var logger = LoggerFor(reporting);
            DumpThreadAborts(logger, LogEventLevel.Fatal, exception, ref messageTemplate);
            logger.Fatal(exception, messageTemplate, propertyValue0, propertyValue1);
        }

        /// <inheritdoc/>
        public void Fatal<T0, T1,T2>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            var logger = LoggerFor(reporting);
            DumpThreadAborts(logger, LogEventLevel.Fatal, exception, ref messageTemplate);
            logger.Fatal(exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <inheritdoc/>
        public void Error(Type reporting, Exception exception, string message)
        {
            var logger = LoggerFor(reporting);
            DumpThreadAborts(logger, LogEventLevel.Error, exception, ref message);
            logger.Error(exception, message);
        }

        /// <inheritdoc/>
        public void Error(Type reporting, Exception exception)
        {
            var logger = LoggerFor(reporting);
            var message = "Exception";
            DumpThreadAborts(logger, LogEventLevel.Error, exception, ref message);
            logger.Error(exception, message);
        }

        /// <inheritdoc/>
        public void Error(Type reporting, string message)
        {
            LoggerFor(reporting).Error(message);
        }

        /// <inheritdoc/>
        public void Error(Type reporting, string messageTemplate, params object[] propertyValues)
        {
            LoggerFor(reporting).Error(messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void Error(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            var logger = LoggerFor(reporting);
            DumpThreadAborts(logger, LogEventLevel.Error, exception, ref messageTemplate);
            logger.Error(exception, messageTemplate, propertyValues);
        }
        /// <inheritdoc/>
        public void Error<T0>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0)
        {
            var logger = LoggerFor(reporting);
            DumpThreadAborts(logger, LogEventLevel.Error, exception, ref messageTemplate);
            logger.Error(exception, messageTemplate, propertyValue0);
        }

        /// <inheritdoc/>
        public void Error<T0, T1>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            var logger = LoggerFor(reporting);
            DumpThreadAborts(logger, LogEventLevel.Error, exception, ref messageTemplate);
            logger.Error(exception, messageTemplate, propertyValue0, propertyValue1);
        }

        /// <inheritdoc/>
        public void Error<T0, T1, T2>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            var logger = LoggerFor(reporting);
            DumpThreadAborts(logger, LogEventLevel.Error, exception, ref messageTemplate);
            logger.Error(exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        private static void DumpThreadAborts(global::Serilog.ILogger logger, LogEventLevel level, Exception exception, ref string messageTemplate)
        {
            var dump = false;

            if (IsTimeoutThreadAbortException(exception))
            {
                messageTemplate += "\r\nThe thread has been aborted, because the request has timed out.";

                // dump if configured, or if stacktrace contains Monitor.ReliableEnter
                dump = Current.Configs.CoreDebug().DumpOnTimeoutThreadAbort || IsMonitorEnterThreadAbortException(exception);

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
                catch (Exception ex)
                {
                    messageTemplate += "\r\nFailed to create a minidump";

                    //Log a new entry (as opposed to appending to same log entry)
                    logger.Write(level, ex, "Failed to create a minidump ({ExType}: {ExMessage})",
                        new object[]{ ex.GetType().FullName, ex.Message });
                }
            }
        }

        private static bool IsMonitorEnterThreadAbortException(Exception exception)
        {
            if (!(exception is ThreadAbortException abort)) return false;

            var stacktrace = abort.StackTrace;
            return stacktrace.Contains("System.Threading.Monitor.ReliableEnter");
        }

        private static bool IsTimeoutThreadAbortException(Exception exception)
        {
            if (!(exception is ThreadAbortException abort)) return false;
            if (abort.ExceptionState == null) return false;

            var stateType = abort.ExceptionState.GetType();
            if (stateType.FullName != "System.Web.HttpApplication+CancelModuleException") return false;

            var timeoutField = stateType.GetField("_timeout", BindingFlags.Instance | BindingFlags.NonPublic);
            if (timeoutField == null) return false;

            return (bool) timeoutField.GetValue(abort.ExceptionState);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, string message)
        {
            LoggerFor(reporting).Warning(message);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, string message, params object[] propertyValues)
        {
            LoggerFor(reporting).Warning(message, propertyValues);
        }

        /// <inheritdoc/>
        public void Warn<T0>(Type reporting, string message, T0 propertyValue0)
        {
            LoggerFor(reporting).Warning(message, propertyValue0);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, Exception exception, string message)
        {
            LoggerFor(reporting).Warning(exception, message);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            LoggerFor(reporting).Warning(exception, messageTemplate, propertyValues);
        }
        /// <inheritdoc/>
        public void Warn<T0>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0)
        {
            var logger = LoggerFor(reporting);
            DumpThreadAborts(logger, LogEventLevel.Warning, exception, ref messageTemplate);
            logger.Warning(exception, messageTemplate, propertyValue0);
        }

        /// <inheritdoc/>
        public void Warn<T0, T1>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            var logger = LoggerFor(reporting);
            DumpThreadAborts(logger, LogEventLevel.Warning, exception, ref messageTemplate);
            logger.Warning(exception, messageTemplate, propertyValue0, propertyValue1);
        }

        /// <inheritdoc/>
        public void Warn<T0, T1, T2>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            var logger = LoggerFor(reporting);
            DumpThreadAborts(logger, LogEventLevel.Warning, exception, ref messageTemplate);
            logger.Warning(exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <inheritdoc/>
        public void Info(Type reporting, string message)
        {
            LoggerFor(reporting).Information(message);
        }

        /// <inheritdoc/>
        public void Info(Type reporting, string messageTemplate, params object[] propertyValues)
        {
            LoggerFor(reporting).Information(messageTemplate, propertyValues);
        }


        /// <inheritdoc/>
        public void Info<T0>(Type reporting, string messageTemplate, T0 propertyValue0)
        {
            LoggerFor(reporting).Information(messageTemplate, propertyValue0);
        }
        /// <inheritdoc/>
        public void Info<T0,T1>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            LoggerFor(reporting).Information(messageTemplate, propertyValue0, propertyValue1);
        }
        /// <inheritdoc/>
        public void Info<T0,T1,T2>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            LoggerFor(reporting).Information(messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <inheritdoc/>
        public void Debug(Type reporting, string message)
        {
            LoggerFor(reporting).Debug(message);
        }

        /// <inheritdoc/>
        public void Debug(Type reporting, string messageTemplate, params object[] propertyValues)
        {
            LoggerFor(reporting).Debug(messageTemplate, propertyValues);
        }
        /// <inheritdoc/>
        public void Debug<T0>(Type reporting, string messageTemplate, T0 propertyValue0)
        {
            LoggerFor(reporting).Debug(messageTemplate, propertyValue0);
        }
        /// <inheritdoc/>
        public void Debug<T0, T1>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            LoggerFor(reporting).Debug(messageTemplate, propertyValue0, propertyValue1);
        }
        /// <inheritdoc/>
        public void Debug<T0, T1, T2>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            LoggerFor(reporting).Debug(messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        /// <inheritdoc/>
        public void Verbose(Type reporting, string message)
        {
            LoggerFor(reporting).Verbose(message);
        }

        /// <inheritdoc/>
        public void Verbose(Type reporting, string messageTemplate, params object[] propertyValues)
        {
            LoggerFor(reporting).Verbose(messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void Verbose<T0>(Type reporting, string messageTemplate, T0 propertyValue0)
        {
            LoggerFor(reporting).Verbose(messageTemplate, propertyValue0);
        }
        /// <inheritdoc/>
        public void Verbose<T0, T1>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            LoggerFor(reporting).Verbose(messageTemplate, propertyValue0, propertyValue1);
        }
        /// <inheritdoc/>
        public void Verbose<T0, T1, T2>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            LoggerFor(reporting).Verbose(messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        public void Dispose()
        {
            Log.CloseAndFlush();
        }
    }
}
