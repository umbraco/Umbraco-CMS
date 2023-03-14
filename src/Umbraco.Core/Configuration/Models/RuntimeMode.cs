namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
/// Represents the configured Umbraco runtime mode.
/// </summary>
public enum RuntimeMode
{
    /// <summary>
    /// The backoffice development mode ensures the runtime is configured for rapidly applying changes within the backoffice.
    /// </summary>
    BackofficeDevelopment = 0,

    /// <summary>
    /// The development mode ensures the runtime is configured for rapidly applying changes.
    /// </summary>
    Development = 1,

    /// <summary>
    /// The production mode ensures optimal performance settings are configured and denies any changes that would require recompilations.
    /// </summary>
    Production = 2
}
