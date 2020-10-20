using System;

namespace Umbraco.Core.Logging
{
    public class LoggingConfiguration : ILoggingConfiguration
    {
        public LoggingConfiguration(string logDirectory)
        {
            LogDirectory = logDirectory ?? throw new ArgumentNullException(nameof(logDirectory));
        }

        public string LogDirectory { get; }
    }
}
