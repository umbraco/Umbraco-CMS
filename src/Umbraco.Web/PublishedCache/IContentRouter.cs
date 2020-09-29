namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Router for content ids
    /// </summary>
    public interface IContentRouter
    {
        /// <summary>
        /// Gets content id identified by a route.
        /// </summary>
        /// <param name="preview">A value indicating whether to consider unpublished content.</param>
        /// <param name="route">The route</param>
        /// <param name="hideTopLevelNode">A value forcing the HideTopLevelNode setting.</param>
        /// <returns>The content, or null.</returns>
        /// <remarks>
        /// <para>A valid route is either a simple path eg <c>/foo/bar/nil</c> or a root node id and a path, eg <c>123/foo/bar/nil</c>.</para>
        /// <para>If <param name="hideTopLevelNode" /> is <c>null</c> then the settings value is used.</para>
        /// <para>The value of <paramref name="preview"/> overrides defaults.</para>
        /// </remarks>
        ContentRoutingResult GetIdByRoute(IPublishedContentCache currentSnapshot,bool preview, string route, bool? hideTopLevelNode, string culture);
        /// <summary>
        /// Gets content id identified by a route.
        /// </summary>
        /// <param name="route">The route</param>
        /// <param name="hideTopLevelNode">A value forcing the HideTopLevelNode setting.</param>
        /// <returns>The content, or null.</returns>
        /// <remarks>
        /// <para>A valid route is either a simple path eg <c>/foo/bar/nil</c> or a root node id and a path, eg <c>123/foo/bar/nil</c>.</para>
        /// <para>If <param name="hideTopLevelNode" /> is <c>null</c> then the settings value is used.</para>
        /// <para>Considers published or unpublished content depending on defaults.</para>
        /// </remarks>
        ContentRoutingResult GetIdByRoute(IPublishedSnapshot currentSnapshot, string route, bool? hideTopLevelNode, string culture);
        /// <summary>
        /// Gets the route for a content identified by its unique identifier.
        /// </summary>
        /// <param name="preview">A value indicating whether to consider unpublished content.</param>
        /// <param name="contentId">The content unique identifier.</param>
        /// <returns>The route.</returns>
        /// <remarks>The value of <paramref name="preview"/> overrides defaults.</remarks>
        string GetRouteById(IPublishedContentCache snapshot, IDomainCache domainCache, bool preview, int contentId, string culture = null);
        // <summary>
        /// Gets the route for a content identified by its unique identifier.
        /// </summary>
        /// <param name="contentId">The content unique identifier.</param>
        /// <returns>The route.</returns>
        /// <remarks>Considers published or unpublished content depending on defaults.</remarks>
        string GetRouteById(bool defaultPreview, IPublishedContentCache snapshot, IDomainCache domainCache, int contentId, string culture = null);

        /// <summary>
        /// Gets the route for a content identified by its alias.
        /// </summary>
        /// <param name="preview">In Preview</param>
        /// <param name="rootNodeId">Domain content id</param>
        /// <param name="culture">Request culture</param>
        /// <param name="alias">Url Alias</param>
        /// <returns>Routing Outcome</returns>
        ContentRoutingResult GetIdByAlias(IPublishedSnapshot currentSnapshot, bool preview, int rootNodeId, string culture, string alias);
    }
}
