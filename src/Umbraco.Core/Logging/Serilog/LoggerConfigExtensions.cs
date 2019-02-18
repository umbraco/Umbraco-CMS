﻿using System;
using System.Web;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Umbraco.Core.Logging.Serilog.Enrichers;

namespace Umbraco.Core.Logging.Serilog
{
    public static class LoggerConfigExtensions
    {
        /// <summary>
        /// This configures Serilog with some defaults
        /// Such as adding ProcessID, Thread, AppDomain etc
        /// It is highly recommended that you keep/use this default in your own logging config customizations
        /// </summary>
        /// <param name="logConfig">A Serilog LoggerConfiguration</param>
        public static LoggerConfiguration MinimalConfiguration(this LoggerConfiguration logConfig)
        {
            global::Serilog.Debugging.SelfLog.Enable(msg => System.Diagnostics.Debug.WriteLine(msg));

            //Set this environment variable - so that it can be used in external config file
            //add key="serilog:write-to:RollingFile.pathFormat" value="%BASEDIR%\logs\log.txt" />
            Environment.SetEnvironmentVariable("BASEDIR", AppDomain.CurrentDomain.BaseDirectory, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("MACHINENAME", Environment.MachineName, EnvironmentVariableTarget.Process);

            logConfig.MinimumLevel.Verbose() //Set to highest level of logging (as any sinks may want to restrict it to Errors only)
                .Enrich.WithProcessId()
                .Enrich.WithProcessName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("AppDomainId", AppDomain.CurrentDomain.Id)
                .Enrich.WithProperty("AppDomainAppId", HttpRuntime.AppDomainAppId.ReplaceNonAlphanumericChars(string.Empty))
                .Enrich.WithProperty("MachineName", Environment.MachineName)
                .Enrich.With<Log4NetLevelMapperEnricher>()
                .Enrich.With<HttpSessionIdEnricher>()
                .Enrich.With<HttpRequestNumberEnricher>()
                .Enrich.With<HttpRequestIdEnricher>();
            
            return logConfig;
        }

        /// <summary>
        /// Outputs a .txt format log at /App_Data/Logs/
        /// </summary>
        /// <param name="logConfig">A Serilog LoggerConfiguration</param>
        /// <param name="minimumLevel">The log level you wish the JSON file to collect - default is Verbose (highest)</param>
        /// <param name="retainedFileCount">The number of days to keep log files. Default is set to null which means all logs are kept</param>
        public static LoggerConfiguration OutputDefaultTextFile(this LoggerConfiguration logConfig, LogEventLevel minimumLevel = LogEventLevel.Verbose, int? retainedFileCount = null)
        {
            //Main .txt logfile - in similar format to older Log4Net output
            //Ends with ..txt as Date is inserted before file extension substring
            logConfig.WriteTo.File($@"{AppDomain.CurrentDomain.BaseDirectory}\App_Data\Logs\UmbracoTraceLog.{Environment.MachineName}..txt",
                    shared: true,
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: minimumLevel,
                    retainedFileCountLimit: null, //Setting to null means we keep all files - default is 31 days
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss,fff} [P{ProcessId}/D{AppDomainId}/T{ThreadId}] {Log4NetLevel}  {SourceContext} - {Message:lj}{NewLine}{Exception}");

            return logConfig;
        }

        /// <summary>
        /// Outputs a CLEF format JSON log at /App_Data/Logs/
        /// </summary>
        /// <param name="logConfig">A Serilog LoggerConfiguration</param>
        /// <param name="minimumLevel">The log level you wish the JSON file to collect - default is Verbose (highest)</param>
        /// <param name="retainedFileCount">The number of days to keep log files. Default is set to null which means all logs are kept</param>
        public static LoggerConfiguration OutputDefaultJsonFile(this LoggerConfiguration logConfig, LogEventLevel minimumLevel = LogEventLevel.Verbose, int? retainedFileCount = null)
        {
            //.clef format (Compact log event format, that can be imported into local SEQ & will make searching/filtering logs easier)
            //Ends with ..txt as Date is inserted before file extension substring
            logConfig.WriteTo.File(new CompactJsonFormatter(), $@"{AppDomain.CurrentDomain.BaseDirectory}\App_Data\Logs\UmbracoTraceLog.{Environment.MachineName}..json",
                shared: true,
                rollingInterval: RollingInterval.Day, //Create a new JSON file every day
                retainedFileCountLimit: retainedFileCount, //Setting to null means we keep all files - default is 31 days
                restrictedToMinimumLevel: minimumLevel);

            return logConfig;
        }

        /// <summary>
        /// Reads settings from /config/serilog.config
        /// That allows the main logging pipeline to be configured
        /// </summary>
        /// <param name="logConfig">A Serilog LoggerConfiguration</param>
        public static LoggerConfiguration ReadFromConfigFile(this LoggerConfiguration logConfig)
        {
            //Read from main serilog.config file
            logConfig.ReadFrom.AppSettings(filePath: AppDomain.CurrentDomain.BaseDirectory + @"\config\serilog.config");

            return logConfig;
        }

        /// <summary>
        /// Reads settings from /config/serilog.user.config
        /// That allows a separate logging pipeline to be configured that will not affect the main Umbraco log
        /// </summary>
        /// <param name="logConfig">A Serilog LoggerConfiguration</param>
        public static LoggerConfiguration ReadFromUserConfigFile(this LoggerConfiguration logConfig)
        {
            //A nested logger - where any user configured sinks via config can not effect the main 'umbraco' logger above
            logConfig.WriteTo.Logger(cfg =>
                cfg.ReadFrom.AppSettings(filePath: AppDomain.CurrentDomain.BaseDirectory + @"\config\serilog.user.config"));

            return logConfig;
        }
    }
}
