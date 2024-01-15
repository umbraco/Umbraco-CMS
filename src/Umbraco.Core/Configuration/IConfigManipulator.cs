namespace Umbraco.Cms.Core.Configuration;

public interface IConfigManipulator
{
    void RemoveConnectionString();

    void SaveConnectionString(string connectionString, string? providerName);

    Task SaveConnectionStringAsync(string connectionString, string? providerName)
    {
        SaveConnectionString(connectionString, providerName);
        return Task.CompletedTask;
    }

    void SaveConfigValue(string itemPath, object value);

    void SaveDisableRedirectUrlTracking(bool disable);

    void SetGlobalId(string id);
}
