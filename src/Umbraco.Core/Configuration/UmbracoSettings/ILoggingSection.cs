using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface ILoggingSection : IUmbracoConfigurationSection
    {
        bool AutoCleanLogs { get; }

        bool EnableLogging { get; }

        bool EnableAsyncLogging { get; }

        int CleaningMiliseconds { get; }

        int MaxLogAge { get; }

        IEnumerable<ILogType> DisabledLogTypes { get; }

        string ExternalLoggerAssembly { get; }

        string ExternalLoggerType { get; }

        bool ExternalLoggerEnableAuditTrail { get; }

        bool ExternalLoggerIsConfigured { get; }
    }
}