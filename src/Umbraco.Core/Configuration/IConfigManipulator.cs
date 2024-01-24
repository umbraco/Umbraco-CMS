namespace Umbraco.Cms.Core.Configuration;

public interface IConfigManipulator
{
    [Obsolete("Use RemoveConnectionStringAsync instead, scheduled for removal in V16.")]
    void RemoveConnectionString();

    [Obsolete("Use SaveConnectionStringAsync instead, scheduled for removal in V16.")]
    void SaveConnectionString(string connectionString, string? providerName);

    [Obsolete("Use SaveConfigValueAsync instead, scheduled for removal in V16.")]
    void SaveConfigValue(string itemPath, object value);

    [Obsolete("Use SaveDisableRedirectUrlTrackingAsync instead, scheduled for removal in V16.")]
    void SaveDisableRedirectUrlTracking(bool disable);

    [Obsolete("Use SetGlobalIdAsync instead, scheduled for removal in V16.")]
    void SetGlobalId(string id);

    /// <summary>
    /// Removes the connection string from the configuration file
    /// </summary>
    /// <returns></returns>
    Task RemoveConnectionStringAsync()
    {
        RemoveConnectionString();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Saves the connection string to the configuration file
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="providerName"></param>
    /// <returns></returns>
    Task SaveConnectionStringAsync(string connectionString, string? providerName)
    {
        SaveConnectionString(connectionString, providerName);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates a value in the configuration file.
    /// <remarks>Will only update an existing key in the configuration file, if it does not exists nothing is saved</remarks>
    /// </summary>
    /// <param name="itemPath">Path to update, uses : as the separator.</param>
    /// <param name="value">The new value.</param>
    /// <returns></returns>
    Task SaveConfigValueAsync(string itemPath, object value)
    {
        SaveConfigValue(itemPath, value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates the disableRedirectUrlTracking value in the configuration file.
    /// <remarks>
    /// Will create the node if it does not already exist.
    /// </remarks>
    /// </summary>
    /// <param name="disable">The value to save.</param>
    /// <returns></returns>
    Task SaveDisableRedirectUrlTrackingAsync(bool disable)
    {
        SaveDisableRedirectUrlTracking(disable);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets the global id in the configuration file.
    /// <remarks>
    /// Will create the node if it does not already exist.
    /// </remarks>
    /// </summary>
    /// <param name="id">The ID to save.</param>
    /// <returns></returns>
    Task SetGlobalIdAsync(string id)
    {
        SetGlobalId(id);
        return Task.CompletedTask;
    }
}
