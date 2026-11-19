namespace Umbraco.Cms.Core.Configuration;

/// <summary>
/// Defines the contract for persisting configuration values back to the underlying JSON configuration files.
/// </summary>
public interface IConfigManipulator
{
    /// <summary>
    /// Saves the Umbraco database connection string to the most specific JSON configuration source available (typically <c>appsettings.{Environment}.json</c>),
    /// so the value lives alongside the environment it applies to. Falls back to <c>appsettings.json</c> when no environment-specific source is present.
    /// </summary>
    /// <param name="connectionString">The connection string to save.</param>
    /// <param name="providerName">The optional provider name to save alongside the connection string.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    Task SaveConnectionStringAsync(string connectionString, string? providerName);

    /// <summary>
    /// Removes the Umbraco database connection string from the most specific JSON configuration source that contains it (typically <c>appsettings.{Environment}.json</c>).
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    Task RemoveConnectionStringAsync();

    /// <summary>
    /// Sets the global site identifier in the base JSON configuration source (typically <c>appsettings.json</c>),
    /// since the value applies across all environments. Creates the node if it does not already exist.
    /// </summary>
    /// <param name="id">The identifier to save.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    Task SetGlobalIdAsync(string id);

    /// <summary>
    /// Sets the imaging HMAC secret key in the base JSON configuration source (typically <c>appsettings.json</c>),
    /// since the value is recommended to be the same across all environments. Creates the node if it does not already exist.
    /// </summary>
    /// <param name="base64Key">The base64-encoded key to save.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    Task SetImagingHmacSecretKeyAsync(string base64Key);
}
