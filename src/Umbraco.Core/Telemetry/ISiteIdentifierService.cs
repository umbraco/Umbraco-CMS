namespace Umbraco.Cms.Core.Telemetry;

/// <summary>
///     Used to get and create the site identifier
/// </summary>
public interface ISiteIdentifierService
{
    /// <summary>
    ///     Tries to get the site identifier
    /// </summary>
    /// <returns>True if success.</returns>
    bool TryGetSiteIdentifier(out Guid siteIdentifier);

    /// <summary>
    ///     Creates the site identifier and writes it to config.
    /// </summary>
    /// <param name="createdGuid">asd.</param>
    /// <returns>True if success.</returns>
    bool TryCreateSiteIdentifier(out Guid createdGuid);

    /// <summary>
    ///     Tries to get the site identifier or otherwise create it if it doesn't exist.
    /// </summary>
    /// <param name="siteIdentifier">The out parameter for the existing or create site identifier.</param>
    /// <returns>True if success.</returns>
    bool TryGetOrCreateSiteIdentifier(out Guid siteIdentifier);
}
