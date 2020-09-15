using System.Collections.Generic;

namespace Umbraco.Core.HealthCheck
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
