namespace Umbraco.Cms.Core.Configuration;

public interface IConfigManipulator
{
    /// <summary>
    /// Removes the connection string from the configuration file
    /// </summary>
    /// <returns></returns>
    Task RemoveConnectionStringAsync();

    /// <summary>
    /// Saves the connection string to the configuration file
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="providerName"></param>
    /// <returns></returns>
    Task SaveConnectionStringAsync(string connectionString, string? providerName);

    /// <summary>
    /// Updates a value in the configuration file.
    /// <remarks>Will only update an existing key in the configuration file, if it does not exists nothing is saved</remarks>
    /// </summary>
    /// <param name="itemPath">Path to update, uses : as the separator.</param>
    /// <param name="value">The new value.</param>
    /// <returns></returns>
    Task SaveConfigValueAsync(string itemPath, object value);

    /// <summary>
    /// Updates the disableRedirectUrlTracking value in the configuration file.
    /// <remarks>
    /// Will create the node if it does not already exist.
    /// </remarks>
    /// </summary>
    /// <param name="disable">The value to save.</param>
    /// <returns></returns>
    Task SaveDisableRedirectUrlTrackingAsync(bool disable);

    /// <summary>
    /// Sets the global id in the configuration file.
    /// <remarks>
    /// Will create the node if it does not already exist.
    /// </remarks>
    /// </summary>
    /// <param name="id">The ID to save.</param>
    /// <returns></returns>
    Task SetGlobalIdAsync(string id);
}
