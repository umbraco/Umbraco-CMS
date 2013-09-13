using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface ILogging
    {
        bool AutoCleanLogs { get; }

        bool EnableLogging { get; }

        bool EnableAsyncLogging { get; }

        int CleaningMiliseconds { get; }

        int MaxLogAge { get; }

        IEnumerable<ILogType> DisabledLogTypes { get; }

        IExternalLogger ExternalLogger { get; }

        bool ExternalLoggerIsConfigured { get; }
    }
}