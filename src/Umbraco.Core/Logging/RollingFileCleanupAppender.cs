using System;
using System.IO;
using log4net.Appender;
using log4net.Util;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// This class will do the exact same thing as the RollingFileAppender that comes from log4net
    /// With the extension, that it is able to do automatic cleanup of the logfiles in the directory where logging happens
    /// 
    /// By specifying the properties MaxLogFileDays and BaseFilePattern, the files will automaticly get deleted when
    /// the logger is configured(typically when the app starts). To utilize this appender swap out the type of the rollingFile appender
    /// that ships with Umbraco, to be Umbraco.Core.Logging.RollingFileCleanupAppender, and add the maxLogFileDays and baseFilePattern elements 
    /// to the configuration i.e.:
    /// 
    /// <example>
    ///  <appender name="rollingFile" type="Log4netAwesomeness.CustomRollingFileAppender, Log4netAwesomeness">
    ///	  <file type="log4net.Util.PatternString" value="App_Data\Logs\UmbracoTraceLog.%property{log4net:HostName}.txt" />
    ///    <lockingModel type="log4net.Appender.FileAppender+MinimalLock" />
    ///    <appendToFile value="true" />
    ///    <rollingStyle value="Date" />
    ///    <maximumFileSize value="5MB" />
    ///    <maxLogFileDays value="5"/>
    ///    <basefilePattern value="UmbracoTraceLog.*.txt.*"/>
    ///    <layout type="log4net.Layout.PatternLayout">
    ///      <conversionPattern value=" %date [P%property{processId}/D%property{appDomainId}/T%thread] %-5level %logger - %message%newline" />
    ///    </layout>
    ///    <layout type="log4net.Layout.PatternLayout">
    ///      <conversionPattern value=" %date [P%property{processId}/D%property{appDomainId}/T%thread] %-5level %logger - %message%newline" />
    ///    </layout>
    ///    <encoding value="utf-8" />
    ///  </appender>
    /// </example>
    /// </summary>
    public class RollingFileCleanupAppender : RollingFileAppender
    {
        public int MaxLogFileDays { get; set; }
        public string BaseFilePattern { get; set; }

        /// <summary>
        /// This override will delete logs older than the specified amount of days
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="append"></param>
        protected override void OpenFile(string fileName, bool append)
        {
            bool cleanup = true;
            // Validate settings and input
            if (MaxLogFileDays <= 0)
            {
                LogLog.Warn(typeof(RollingFileCleanupAppender), "Parameter 'MaxLogFileDays' needs to be a positive integer, aborting cleanup");
                cleanup = false;
            }

            if (string.IsNullOrWhiteSpace(BaseFilePattern))
            {
                LogLog.Warn(typeof(RollingFileCleanupAppender), "Parameter 'BaseFilePattern' is empty, aborting cleanup");
                cleanup = false;
            }
            // grab the directory we are logging to, as this is were we will search for older logfiles
            var logFolder = Path.GetDirectoryName(fileName);
            if (Directory.Exists(logFolder) == false)
            {
                LogLog.Warn(typeof(RollingFileCleanupAppender), string.Format("Directory '{0}' for logfiles does not exist, aborting cleanup", logFolder));
                cleanup = false;
            }
            // If everything is validated, we can do the actual cleanup
            if (cleanup)
            {
                Cleanup(logFolder);
            }

            base.OpenFile(fileName, append);
        }

        private void Cleanup(string directoryPath)
        {
            // only take files that matches the pattern we are using i.e. UmbracoTraceLog.*.txt.*
            string[] logFiles = Directory.GetFiles(directoryPath, BaseFilePattern);
            LogLog.Debug(typeof(RollingFileCleanupAppender), string.Format("Found {0} files that matches the baseFilePattern: '{1}'", logFiles.Length, BaseFilePattern));

            foreach (var logFile in logFiles)
            {
                DateTime lastAccessTime = System.IO.File.GetLastWriteTimeUtc(logFile);
                // take the value from the config file
                if (lastAccessTime < DateTime.Now.AddDays(-MaxLogFileDays))
                {
                    LogLog.Debug(typeof(RollingFileCleanupAppender), string.Format("Deleting file {0} as its lastAccessTime is older than {1} days speficied by MaxLogFileDays", logFile, MaxLogFileDays));
                    base.DeleteFile(logFile);
                }
            }
        }
    }
}
