namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Represents the configured Umbraco runtime mode.
    /// </summary>
    public enum RuntimeMode
    {
        /// <summary>
        /// The development mode ensures the runtime is configured for rapidly applying changes.
        /// </summary>
        Development,

        /// <summary>
        /// The production mode ensures optimal performance settings are configured and denies any changes that would require recompilations.
        /// </summary>
        Production
    }
}
