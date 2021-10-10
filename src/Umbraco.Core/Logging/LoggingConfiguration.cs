using System;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Logging
{
    [Obsolete("Please use UmbracoLoggerConfiguration instead.")]
    public class LoggingConfiguration : ILoggingConfiguration
    {
        public LoggingConfiguration(string logDirectory)
        {
            LogDirectory = logDirectory ?? throw new ArgumentNullException(nameof(logDirectory));
        }

        public string LogDirectory { get; }
    }

    public class UmbracoLoggerConfiguration : ILoggingConfiguration
    {
        public string BasePath { get; }
        public string LogDirectory { get; }
        public string ApplicationId { get; }

        public UmbracoLoggerConfiguration(string basePath, string logDirectory, string applicationId)
        {
            BasePath = basePath.TrimEnd("\\") ?? throw new ArgumentNullException(nameof(basePath));
            LogDirectory = logDirectory ?? throw new ArgumentNullException(nameof(logDirectory));
            ApplicationId = applicationId ?? throw new ArgumentNullException(nameof(applicationId));
        }
    }
}
