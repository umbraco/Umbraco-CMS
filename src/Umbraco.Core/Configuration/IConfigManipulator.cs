namespace Umbraco.Cms.Core.Configuration;

/// <summary>
/// Defines the contract for persisting configuration values back to the underlying JSON configuration files.
/// </summary>
public interface IConfigManipulator
{
    /// <summary>
    /// Saves the Umbraco database connection string to the most specific JSON configuration source available
    /// (typically <c>appsettings.{Environment}.json</c>), so the value lives alongside the environment it applies to.
    /// Falls back to <c>appsettings.json</c> when no environment-specific source is present.
    /// </summary>
    /// <param name="connectionString">The connection string to save.</param>
    /// <param name="providerName">The optional provider name to save alongside the connection string.</param>
    Task SaveConnectionStringAsync(string connectionString, string? providerName);

    /// <summary>
    /// Removes the Umbraco database connection string from the most specific JSON configuration source that contains it
    /// (typically <c>appsettings.{Environment}.json</c>).
    /// </summary>
    Task RemoveConnectionStringAsync();

    /// <summary>
    /// Updates an existing value in the base JSON configuration source (typically <c>appsettings.json</c>).
    /// </summary>
    /// <remarks>
    /// Will only update an existing key; if the key is not already present, nothing is saved.
    /// </remarks>
    /// <param name="itemPath">The path to update, using <c>:</c> as the separator.</param>
    /// <param name="value">The new value.</param>
    [Obsolete("This method is no longer used by Umbraco. Scheduled for removal in Umbraco 19.")]
    Task SaveConfigValueAsync(string itemPath, object value);

    /// <summary>
    /// Updates the redirect URL tracking setting in the base JSON configuration source (typically <c>appsettings.json</c>),
    /// since the value applies across all environments. Creates the node if it does not already exist.
    /// </summary>
    /// <param name="disable">The value to save.</param>
    Task SaveDisableRedirectUrlTrackingAsync(bool disable);

    /// <summary>
    /// Sets the global site identifier in the base JSON configuration source (typically <c>appsettings.json</c>),
    /// since the value applies across all environments. Creates the node if it does not already exist.
    /// </summary>
    /// <param name="id">The identifier to save.</param>
    Task SetGlobalIdAsync(string id);

    /// <summary>
    /// Sets the imaging HMAC secret key in the base JSON configuration source (typically <c>appsettings.json</c>),
    /// since the value is recommended to be the same across all environments. Creates the node if it does not already exist.
    /// </summary>
    /// <param name="base64Key">The base64-encoded key to save.</param>
    // TODO (V18): Remove the default implementation.
    Task SetImagingHmacSecretKeyAsync(string base64Key)
        => Task.CompletedTask;
}
