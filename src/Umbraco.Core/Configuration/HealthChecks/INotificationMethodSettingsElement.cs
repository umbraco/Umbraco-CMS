namespace Umbraco.Core.Configuration.HealthChecks
{
    public interface INotificationMethodSettingsElement
    {
        string Key { get; }
        string Value { get; }
    }
}