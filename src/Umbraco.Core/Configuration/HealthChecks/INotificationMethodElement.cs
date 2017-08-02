namespace Umbraco.Core.Configuration.HealthChecks
{
    public interface INotificationMethodElement
    {
        string Alias { get; }
        bool Enabled { get; }
        HealthCheckNotificationVerbosity Verbosity { get; }
        bool FailureOnly { get; }
        NotificationMethodSettingsElementCollection Settings { get; }
    }
}
