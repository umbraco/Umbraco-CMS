using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Compact;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Logging.Serilog.Enrichers;
using Umbraco.Cms.Infrastructure.Logging.Serilog;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring Serilog-based logging within Umbraco applications.
    /// </summary>
    public static class LoggerConfigExtensions
    {
        /// <summary>
        /// Configures Serilog with recommended default enrichers and settings for Umbraco applications.
        /// This includes adding enrichers such as ProcessID, Thread, AppDomain, and others.
        /// It is highly recommended to retain or use this default configuration when customizing logging in your application.
        /// </summary>
        /// <param name="logConfig">The <see cref="LoggerConfiguration"/> instance to configure.</param>
        /// <param name="hostingEnvironment">The current Umbraco hosting environment.</param>
        /// <param name="loggingConfiguration">The logging configuration settings.</param>
        /// <param name="configuration">The application configuration instance.</param>
        /// <returns>The configured <see cref="LoggerConfiguration"/> instance with Umbraco defaults applied.</returns>
        [Obsolete("Please use an alternative method. Scheduled for removal from Umbraco 13.")]
        public static LoggerConfiguration MinimalConfiguration(
            this LoggerConfiguration logConfig,
            Umbraco.Cms.Core.Hosting.IHostingEnvironment hostingEnvironment,
            ILoggingConfiguration loggingConfiguration,
            IConfiguration configuration)
        {
            return MinimalConfiguration(logConfig, hostingEnvironment, loggingConfiguration, configuration, out _);
        }

        /// <summary>
        /// Configures Serilog with recommended default enrichers and settings, such as adding ProcessID, Thread, and AppDomain information.
        /// It is highly recommended to retain this default configuration when customizing logging in your application.
        /// </summary>
        /// <param name="logConfig">The <see cref="LoggerConfiguration"/> instance to configure.</param>
        /// <param name="hostingEnvironment">Provides information about the hosting environment.</param>
        /// <param name="loggingConfiguration">The logging configuration settings.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="umbFileConfiguration">When this method returns, contains the Umbraco file configuration.</param>
        /// <returns>The configured <see cref="LoggerConfiguration"/> instance.</returns>
        [Obsolete("Please use an alternative method. Scheduled for removal from Umbraco 13.")]
        public static LoggerConfiguration MinimalConfiguration(
            this LoggerConfiguration logConfig,
            Umbraco.Cms.Core.Hosting.IHostingEnvironment hostingEnvironment,
            ILoggingConfiguration loggingConfiguration,
            IConfiguration configuration,
            out UmbracoFileConfiguration umbFileConfiguration)
        {
            Serilog.Debugging.SelfLog.Enable(msg => System.Diagnostics.Debug.WriteLine(msg));

            //Set this environment variable - so that it can be used in external config file
            //add key="serilog:write-to:RollingFile.pathFormat" value="%BASEDIR%\logs\log.txt" />
            Environment.SetEnvironmentVariable("BASEDIR", hostingEnvironment.MapPathContentRoot("/").TrimEnd(Path.DirectorySeparatorChar), EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("UMBLOGDIR", loggingConfiguration.LogDirectory, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("MACHINENAME", Environment.MachineName, EnvironmentVariableTarget.Process);

            logConfig.MinimumLevel.Verbose() //Set to highest level of logging (as any sinks may want to restrict it to Errors only)
                .Enrich.WithProcessId()
                .Enrich.WithProcessName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("ApplicationId", hostingEnvironment.ApplicationId) // Updated later by ApplicationIdEnricher
                .Enrich.WithProperty("MachineName", Environment.MachineName)
                .Enrich.With<Log4NetLevelMapperEnricher>()
                .Enrich.FromLogContext(); // allows us to dynamically enrich

            //This is not optimal, but seems to be the only way if we do not make an Serilog.Sink.UmbracoFile sink all the way.
            var umbracoFileConfiguration = new UmbracoFileConfiguration(configuration);

            umbFileConfiguration = umbracoFileConfiguration;

            logConfig.WriteTo.UmbracoFile(
                path: umbracoFileConfiguration.GetPath(loggingConfiguration.LogDirectory),
                fileSizeLimitBytes: umbracoFileConfiguration.FileSizeLimitBytes,
                restrictedToMinimumLevel: umbracoFileConfiguration.RestrictedToMinimumLevel,
                rollingInterval: umbracoFileConfiguration.RollingInterval,
                flushToDiskInterval: umbracoFileConfiguration.FlushToDiskInterval,
                rollOnFileSizeLimit: umbracoFileConfiguration.RollOnFileSizeLimit,
                retainedFileCountLimit: umbracoFileConfiguration.RetainedFileCountLimit);

            return logConfig;
        }

        /// <summary>
        /// Configures Serilog with recommended default enrichers and settings, such as adding ProcessID, Thread, and AppDomain information.
        /// It is highly recommended to retain this default configuration when customizing your own logging setup.
        /// </summary>
        /// <param name="logConfig">The <see cref="LoggerConfiguration"/> instance to configure.</param>
        /// <param name="hostingEnvironment">The Umbraco hosting environment.</param>
        /// <param name="loggingConfiguration">The logging configuration settings.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The configured <see cref="LoggerConfiguration"/> instance.</returns>
        public static LoggerConfiguration MinimalConfiguration(
            this LoggerConfiguration logConfig,
            IHostEnvironment hostEnvironment,
            ILoggingConfiguration loggingConfiguration,
            UmbracoFileConfiguration umbracoFileConfiguration)
        {
            Serilog.Debugging.SelfLog.Enable(msg => System.Diagnostics.Debug.WriteLine(msg));

            //Set this environment variable - so that it can be used in external config file
            //add key="serilog:write-to:RollingFile.pathFormat" value="%BASEDIR%\logs\log.txt" />
            Environment.SetEnvironmentVariable("BASEDIR", hostEnvironment.MapPathContentRoot("/").TrimEnd("\\"), EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("UMBLOGDIR", loggingConfiguration.LogDirectory, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("MACHINENAME", Environment.MachineName, EnvironmentVariableTarget.Process);

            logConfig.MinimumLevel.Verbose() //Set to highest level of logging (as any sinks may want to restrict it to Errors only)
                .Enrich.WithProcessId()
                .Enrich.WithProcessName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("ApplicationId", hostEnvironment.GetTemporaryApplicationId()) // Updated later by ApplicationIdEnricher
                .Enrich.WithProperty("MachineName", Environment.MachineName)
                .Enrich.With<Log4NetLevelMapperEnricher>()
                .Enrich.FromLogContext(); // allows us to dynamically enrich

            if (umbracoFileConfiguration.Enabled)
            {
                logConfig.WriteTo.UmbracoFile(
                    path: umbracoFileConfiguration.GetPath(loggingConfiguration.LogDirectory, loggingConfiguration.LogFileNameFormat, loggingConfiguration.GetLogFileNameFormatArguments()),
                    fileSizeLimitBytes: umbracoFileConfiguration.FileSizeLimitBytes,
                    restrictedToMinimumLevel: umbracoFileConfiguration.RestrictedToMinimumLevel,
                    rollingInterval: umbracoFileConfiguration.RollingInterval,
                    flushToDiskInterval: umbracoFileConfiguration.FlushToDiskInterval,
                    rollOnFileSizeLimit: umbracoFileConfiguration.RollOnFileSizeLimit,
                    retainedFileCountLimit: umbracoFileConfiguration.RetainedFileCountLimit);
            }

            return logConfig;
        }

        /// <summary>
        /// Outputs a .txt format log at /App_Data/Logs/ in a format similar to the older Log4Net output.
        /// </summary>
        /// <param name="logConfig">The Serilog LoggerConfiguration to configure.</param>
        /// <param name="hostingEnvironment">The hosting environment used to resolve the log file path.</param>
        /// <param name="minimumLevel">The minimum log level to write to the text file; default is Verbose (logs all events).</param>
        /// <returns>The updated LoggerConfiguration.</returns>
        [Obsolete("Scheduled for removal from Umbraco 13.")]
        public static LoggerConfiguration OutputDefaultTextFile(
            this LoggerConfiguration logConfig,
            Umbraco.Cms.Core.Hosting.IHostingEnvironment hostingEnvironment,
            LogEventLevel minimumLevel = LogEventLevel.Verbose)
        {
            //Main .txt logfile - in similar format to older Log4Net output
            //Ends with ..txt as Date is inserted before file extension substring
            logConfig.WriteTo.File(
                Path.Combine(hostingEnvironment.MapPathContentRoot(Cms.Core.Constants.SystemDirectories.LogFiles), $"UmbracoTraceLog.{Environment.MachineName}..txt"),
                shared: true,
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: minimumLevel,
                retainedFileCountLimit: null, //Setting to null means we keep all files - default is 31 days
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss,fff} [P{ProcessId}/D{AppDomainId}/T{ThreadId}] {Log4NetLevel}  {SourceContext} - {Message:lj}{NewLine}{Exception}");

            return logConfig;
        }

        /// <summary>
        /// Outputs a .txt format log at /App_Data/Logs/
        /// </summary>
        /// <param name="logConfig">A Serilog LoggerConfiguration</param>
        /// <param name="hostEnvironment"></param>
        /// <param name="loggingSettings"></param>
        /// <param name="minimumLevel">The log level you wish the JSON file to collect - default is Verbose (highest)</param>
        public static LoggerConfiguration OutputDefaultTextFile(
            this LoggerConfiguration logConfig,
            IHostEnvironment hostEnvironment,
            LoggingSettings loggingSettings,
            LogEventLevel minimumLevel = LogEventLevel.Verbose)
        {
            //Main .txt logfile - in similar format to older Log4Net output
            //Ends with ..txt as Date is inserted before file extension substring
            logConfig.WriteTo.File(
                Path.Combine(loggingSettings.GetAbsoluteLoggingPath(hostEnvironment),  $"UmbracoTraceLog.{Environment.MachineName}..txt"),
                shared: true,
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: minimumLevel,
                retainedFileCountLimit: null, //Setting to null means we keep all files - default is 31 days
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss,fff} [P{ProcessId}/D{AppDomainId}/T{ThreadId}] {Log4NetLevel}  {SourceContext} - {Message:lj}{NewLine}{Exception}");

            return logConfig;
        }

        /// <summary>
        /// Configures a Serilog file sink for logging with Umbraco-specific defaults.
        /// </summary>
        /// <param name="configuration">The logger sink configuration to extend.</param>
        /// <param name="path">The file path where log entries will be written.</param>
        /// <param name="formatter">An optional text formatter for log entries. If not specified, a compact JSON formatter is used.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required to write to the sink. Defaults to Verbose.</param>
        /// <param name="levelSwitch">An optional level switch to dynamically control the minimum log level.</param>
        /// <param name="fileSizeLimitBytes">The maximum size, in bytes, for a single log file before rolling. Defaults to 1 GB.</param>
        /// <param name="flushToDiskInterval">The interval at which log entries are flushed to disk. If null, the default Serilog behavior is used.</param>
        /// <param name="rollingInterval">The interval at which log files are rolled (e.g., daily). Defaults to daily rolling.</param>
        /// <param name="rollOnFileSizeLimit">If true, rolls the log file when the size limit is reached.</param>
        /// <param name="retainedFileCountLimit">The maximum number of log files to retain. Older files are deleted. Defaults to 31.</param>
        /// <param name="encoding">The text encoding to use for the log file. If null, UTF-8 is used.</param>
        /// <returns>The logger configuration with the file sink applied.</returns>
        /// <remarks>
        /// Used in configuration files. If this method is renamed or moved to another assembly, the configuration file must also be updated accordingly.
        /// </remarks>
        public static LoggerConfiguration UmbracoFile(
            this LoggerSinkConfiguration configuration,
            string path,
            ITextFormatter? formatter = null,
            LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
            LoggingLevelSwitch? levelSwitch = null,
            long? fileSizeLimitBytes = 1073741824,
            TimeSpan? flushToDiskInterval = null,
            RollingInterval rollingInterval = RollingInterval.Day,
            bool rollOnFileSizeLimit = false,
            int? retainedFileCountLimit = 31,
            Encoding? encoding = null)
        {
            formatter ??= new CompactJsonFormatter();

            /* Async sink has an event buffer of 10k events (by default) so we're not constantly thrashing the disk.
             * I noticed that with File buffered + large number of log entries (global minimum Debug)
             * an ungraceful shutdown would consistently result in output that just stops halfway through an entry.
             * with buffered false on the inner sink ungraceful shutdowns still don't seem to wreck the file.
             */
            return configuration.Async(
                cfg =>
                    cfg.File(
                        formatter,
                        path,
                        restrictedToMinimumLevel,
                        fileSizeLimitBytes,
                        levelSwitch,
                        buffered: false, // see notes above.
                        shared: false,
                        flushToDiskInterval,
                        rollingInterval,
                        rollOnFileSizeLimit,
                        retainedFileCountLimit,
                        encoding,
                        null));
        }

        /// <summary>
        /// Configures the logger to output a CLEF (Compact Log Event Format) JSON log file to the <c>/App_Data/Logs/</c> directory.
        /// </summary>
        /// <param name="logConfig">The Serilog <see cref="LoggerConfiguration"/> to configure.</param>
        /// <param name="hostingEnvironment">The current Umbraco hosting environment, used to resolve the log file path.</param>
        /// <param name="loggingConfiguration">The logging configuration settings.</param>
        /// <param name="minimumLevel">The minimum <see cref="Serilog.Events.LogEventLevel"/> to log. Defaults to <c>Verbose</c> (logs all events).</param>
        /// <param name="retainedFileCount">The number of days to retain log files. If <c>null</c>, all log files are kept indefinitely.</param>
        /// <returns>The configured <see cref="LoggerConfiguration"/> instance.</returns>
        [Obsolete("Scheduled for removal from Umbraco 13.")]
        public static LoggerConfiguration OutputDefaultJsonFile(
            this LoggerConfiguration logConfig,
            Umbraco.Cms.Core.Hosting.IHostingEnvironment hostingEnvironment,
            ILoggingConfiguration loggingConfiguration,
            LogEventLevel minimumLevel = LogEventLevel.Verbose,
            int? retainedFileCount = null)
        {
            // .clef format (Compact log event format, that can be imported into local SEQ & will make searching/filtering logs easier)
            // Ends with ..txt as Date is inserted before file extension substring
            logConfig.WriteTo.File(
                new CompactJsonFormatter(),
                Path.Combine(hostingEnvironment.MapPathContentRoot(Cms.Core.Constants.SystemDirectories.LogFiles), $"UmbracoTraceLog.{Environment.MachineName}..json"),
                shared: true,
                rollingInterval: RollingInterval.Day, // Create a new JSON file every day
                retainedFileCountLimit: retainedFileCount, // Setting to null means we keep all files - default is 31 days
                restrictedToMinimumLevel: minimumLevel);

            return logConfig;
        }

        /// <summary>
        /// Configures the logger to output logs in CLEF (Compact Log Event Format) JSON format to the <c>/App_Data/Logs/</c> directory.
        /// </summary>
        /// <param name="logConfig">The Serilog <see cref="LoggerConfiguration"/> to configure.</param>
        /// <param name="hostEnvironment">The hosting environment used to resolve the log file path.</param>
        /// <param name="loggingSettings">The logging configuration settings.</param>
        /// <param name="minimumLevel">The minimum log event level to write to the JSON file. Default is <c>Verbose</c> (logs all events).</param>
        /// <param name="retainedFileCount">The number of days to retain log files. If <c>null</c>, all log files are kept indefinitely.</param>
        /// <returns>The configured <see cref="LoggerConfiguration"/> instance.</returns>
        public static LoggerConfiguration OutputDefaultJsonFile(
            this LoggerConfiguration logConfig,
            IHostEnvironment hostEnvironment,
            LoggingSettings loggingSettings,
            LogEventLevel minimumLevel = LogEventLevel.Verbose,
            int? retainedFileCount = null)
        {
            // .clef format (Compact log event format, that can be imported into local SEQ & will make searching/filtering logs easier)
            // Ends with ..txt as Date is inserted before file extension substring
            logConfig.WriteTo.File(
                new CompactJsonFormatter(),
                Path.Combine(loggingSettings.GetAbsoluteLoggingPath(hostEnvironment) ,$"UmbracoTraceLog.{Environment.MachineName}..json"),
                shared: true,
                rollingInterval: RollingInterval.Day, // Create a new JSON file every day
                retainedFileCountLimit: retainedFileCount, // Setting to null means we keep all files - default is 31 days
                restrictedToMinimumLevel: minimumLevel);

            return logConfig;
        }
    }
}
