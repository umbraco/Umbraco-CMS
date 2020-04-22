using System;

namespace Umbraco.Core.Logging
{
    public class LoggingConfiguration : ILoggingConfiguration
    {
        public LoggingConfiguration(string logDirectory, string logConfigurationFile, string userLogConfigurationFile)
        {
            LogDirectory = logDirectory ?? throw new ArgumentNullException(nameof(logDirectory));
            LogConfigurationFile = logConfigurationFile ?? throw new ArgumentNullException(nameof(logConfigurationFile));
            UserLogConfigurationFile = userLogConfigurationFile ?? throw new ArgumentNullException(nameof(userLogConfigurationFile));
        }

        public string LogDirectory { get; }

        public string LogConfigurationFile { get; }

        public string UserLogConfigurationFile { get; }
    }
}
