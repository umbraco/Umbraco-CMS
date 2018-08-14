using System;
using System.Web;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Umbraco.Core.Logging.SerilogExtensions
{
    public static class LoggerConfigExtensions
    {
        public static LoggerConfiguration MinimalConfiguration(this LoggerConfiguration logConfig)
        {
            Serilog.Debugging.SelfLog.Enable(msg => System.Diagnostics.Debug.WriteLine(msg));

            //Set this environment variable - so that it can be used in external config file
            //add key="serilog:write-to:RollingFile.pathFormat" value="%BASEDIR%\logs\log.txt" />
            Environment.SetEnvironmentVariable("BASEDIR", AppDomain.CurrentDomain.BaseDirectory, EnvironmentVariableTarget.Process);
            
            logConfig.MinimumLevel.Verbose() //Set to highest level of logging (as any sinks may want to restrict it to Errors only)
                .Enrich.WithProcessId()
                .Enrich.WithProcessName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("AppDomainId", AppDomain.CurrentDomain.Id)
                .Enrich.WithProperty("AppDomainAppId", HttpRuntime.AppDomainAppId.ReplaceNonAlphanumericChars(string.Empty))
                .Enrich.With<Log4NetLevelMapperEnricher>();

            return logConfig;
        }

        public static LoggerConfiguration OutputDefaultTextFile(this LoggerConfiguration logConfig)
        {
            //Main .txt logfile - in similar format to older Log4Net output
            //Ends with ..txt as Date is inserted before file extension substring
            logConfig.WriteTo.File($@"{AppDomain.CurrentDomain.BaseDirectory}\App_Data\Logs\UmbracoTraceLog.{Environment.MachineName}..txt",
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Verbose,
                    retainedFileCountLimit: null, //Setting to null means we keep all files - default is 31 days
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss,fff} [P{ProcessId}/D{AppDomainId}/T{ThreadId}] {Log4NetLevel}  {SourceContext} - {Message:lj}{NewLine}{Exception}");

            return logConfig;
        }

        public static LoggerConfiguration OutputDefaultJsonFile(this LoggerConfiguration logConfig)
        {
            //.clef format (Compact log event format, that can be imported into local SEQ & will make searching/filtering logs easier)
            //Ends with ..txt as Date is inserted before file extension substring
            logConfig.WriteTo.File(new CompactJsonFormatter(), $@"{AppDomain.CurrentDomain.BaseDirectory}\App_Data\Logs\UmbracoTraceLog.{Environment.MachineName}..json",
                rollingInterval: RollingInterval.Day, //Create a new JSON file every day
                retainedFileCountLimit: null, //Setting to null means we keep all files - default is 31 days
                restrictedToMinimumLevel: LogEventLevel.Verbose);

            return logConfig;
        }

        public static LoggerConfiguration ReadFromConfigFile(this LoggerConfiguration logConfig)
        {
            //Read from main serilog.config file
            logConfig.ReadFrom.AppSettings(filePath: AppDomain.CurrentDomain.BaseDirectory + @"\config\serilog.config");

            return logConfig;
        }

        public static LoggerConfiguration ReadFromUserConfigFile(this LoggerConfiguration logConfig)
        {
            //A nested logger - where any user configured sinks via config can not effect the main 'umbraco' logger above
            logConfig.WriteTo.Logger(cfg =>
                cfg.ReadFrom.AppSettings(filePath: AppDomain.CurrentDomain.BaseDirectory + @"\config\serilog.user.config"));

            return logConfig;
        }
    }
}
