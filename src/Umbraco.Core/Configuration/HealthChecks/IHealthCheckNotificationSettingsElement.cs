namespace Umbraco.Core.Configuration.HealthChecks
{
    public interface IHealthCheckNotificationSettingsElement
    {
        bool Enabled { get; }
        string FirstRunTime { get; }
        int PeriodInHours { get; }
        NotificationMethodsElementCollection NotificationMethods { get; }
        DisabledHealthChecksElementCollection DisabledChecks { get; }
    }
}
