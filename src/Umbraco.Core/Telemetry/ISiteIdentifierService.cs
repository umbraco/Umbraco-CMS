using System;

namespace Umbraco.Core.Telemetry
{
    /// <summary>
    /// Used to get and create the site identifier
    /// </summary>
    public interface ISiteIdentifierService
    {

        /// <summary>
        /// Tries to get the site identifier
        /// </summary>
        /// <returns></returns>
        bool TryGetSiteIdentifier(out Guid siteIdentifier);

        /// <summary>
        /// Creates the site identifier and writes it to config.
        /// </summary>
        bool TryCreateSiteIdentifier(out Guid createdGuid);
    }
}
