namespace Umbraco.Core.Configuration.HealthChecks
{
    public interface INotificationMethodSettings
    {
        string Key { get; }
        string Value { get; }
    }
}
