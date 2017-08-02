namespace Umbraco.Core.Configuration.HealthChecks
{
    public interface IHealthChecksSection
    {
        DisabledHealthChecksElementCollection DisabledChecks { get; }
        HealthCheckNotificationSettingsElement NotificationSettings { get; }
    }
}