using System.Collections.Generic;
using Umbraco.Web.Routing;

namespace Umbraco.Web.PublishedCache
{
    public interface IDomainCache
    {
        /// <summary>
        /// Returns all <see cref="Domain"/> in the current domain cache including any domains that may be referenced by content items that are no longer published
        /// </summary>
        /// <param name="includeWildcards"></param>
        /// <returns></returns>
        IEnumerable<Domain> GetAll(bool includeWildcards);

        /// <summary>
        /// Returns all assigned <see cref="Domain"/> for the content id specified even if the content item is not published
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="includeWildcards"></param>
        /// <returns></returns>
        IEnumerable<Domain> GetAssigned(int contentId, bool includeWildcards);

        string DefaultCulture { get; }
    }
}
