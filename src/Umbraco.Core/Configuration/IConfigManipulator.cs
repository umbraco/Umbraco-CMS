namespace Umbraco.Cms.Core.Configuration;

public interface IConfigManipulator
{
    void RemoveConnectionString();

    void SaveConnectionString(string connectionString, string? providerName);

    void SaveConfigValue(string itemPath, object value);

    void SaveDisableRedirectUrlTracking(bool disable);

    void SetGlobalId(string id);

    Task RemoveConnectionStringAsync()
    {
        RemoveConnectionString();
        return Task.CompletedTask;
    }

    Task SaveConnectionStringAsync(string connectionString, string? providerName)
    {
        SaveConnectionString(connectionString, providerName);
        return Task.CompletedTask;
    }

    Task SaveConfigValueAsync(string itemPath, object value);
}
