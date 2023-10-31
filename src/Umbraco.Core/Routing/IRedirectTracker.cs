using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Routing
{
    /// <summary>
    /// Determines and records redirects for a content item following an update that may change it's public URL.
    /// </summary>
    public interface IRedirectTracker
    {
        /// <summary>
        /// Stores the existing routes for a content item before update.
        /// </summary>
        /// <param name="entity">The content entity updated.</param>
        /// <param name="oldRoutes">The dictionary of routes for population.</param>
        void StoreOldRoute(IContent entity, Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> oldRoutes);

        /// <summary>
        /// Creates appropriate redirects for the content item following an update.
        /// </summary>
        /// <param name="oldRoutes">The populated dictionary of old routes;</param>
        void CreateRedirects(IDictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> oldRoutes);
    }
}
