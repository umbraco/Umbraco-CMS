using System.Collections.Generic;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public interface INotificationMethod
    {
        string Alias { get; }
        bool Enabled { get; }
        HealthCheckNotificationVerbosity Verbosity { get; }
        bool FailureOnly { get; }
        IReadOnlyDictionary<string, INotificationMethodSettings> Settings { get; }
    }
}
