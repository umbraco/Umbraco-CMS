using System;
using System.Collections.Generic;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Cache;

namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Creates and manages <see cref="IPublishedSnapshot"/> instances.
    /// </summary>
    public interface IPublishedSnapshotService : IDisposable
    {
        #region PublishedSnapshot

        /* Various places (such as Node) want to access the XML content, today as an XmlDocument
         * but to migrate to a new cache, they're migrating to an XPathNavigator. Still, they need
         * to find out how to get that navigator.
         *
         * Because a cache such as NuCache is contextual i.e. it has a "snapshot" thing and remains
         * consistent over the snapshot, the navigator has to come from the "current" snapshot.
         *
         * So although everything should be injected... we also need a notion of "the current published
         * snapshot". This is provided by the IPublishedSnapshotAccessor.
         *
         */

        /// <summary>
        /// Creates a published snapshot.
        /// </summary>
        /// <param name="previewToken">A preview token, or <c>null</c> if not previewing.</param>
        /// <returns>A published snapshot.</returns>
        /// <remarks>If <paramref name="previewToken"/> is null, the snapshot is not previewing, else it
        /// is previewing, and what is or is not visible in preview depends on the content of the token,
        /// which is not specified and depends on the actual published snapshot service implementation.</remarks>
        IPublishedSnapshot CreatePublishedSnapshot(string previewToken);

        /// <summary>
        /// Gets the published snapshot accessor.
        /// </summary>
        IPublishedSnapshotAccessor PublishedSnapshotAccessor { get; }

        /// <summary>
        /// Ensures that the published snapshot has the proper environment to run.
        /// </summary>
        /// <param name="errors">The errors, if any.</param>
        /// <returns>A value indicating whether the published snapshot has the proper environment to run.</returns>
        bool EnsureEnvironment(out IEnumerable<string> errors);

        #endregion

        #region Rebuild

        /// <summary>
        /// Rebuilds internal caches (but does not reload).
        /// </summary>
        /// <remarks>
        /// <para>Forces the snapshot service to rebuild its internal caches. For instance, some caches
        /// may rely on a database table to store pre-serialized version of documents.</para>
        /// <para>This does *not* reload the caches. Caches need to be reloaded, for instance via
        /// <see cref="DistributedCache" /> RefreshAllPublishedSnapshot method.</para>
        /// </remarks>
        void Rebuild();

        #endregion

        #region Preview

        /* Later on we can imagine that EnterPreview would handle a "level" that would be either
         * the content only, or the content's branch, or the whole tree + it could be possible
         * to register filters against the factory to filter out which nodes should be preview
         * vs non preview.
         *
         * EnterPreview() returns the previewToken. It is up to callers to store that token
         * wherever they want, most probably in a cookie.
         *
         */

        /// <summary>
        /// Enters preview for specified user and content.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="contentId">The content identifier.</param>
        /// <returns>A preview token.</returns>
        /// <remarks>
        /// <para>Tells the caches that they should prepare any data that they would be keeping
        /// in order to provide preview to a give user. In the Xml cache this means creating the Xml
        /// file, though other caches may do things differently.</para>
        /// <para>Does not handle the preview token storage (cookie, etc) that must be handled separately.</para>
        /// </remarks>
        string EnterPreview(IUser user, int contentId);

        /// <summary>
        /// Refreshes preview for a specified content.
        /// </summary>
        /// <param name="previewToken">The preview token.</param>
        /// <param name="contentId">The content identifier.</param>
        /// <remarks>Tells the caches that they should update any data that they would be keeping
        /// in order to provide preview to a given user. In the Xml cache this means updating the Xml
        /// file, though other caches may do things differently.</remarks>
        void RefreshPreview(string previewToken, int contentId);

        /// <summary>
        /// Exits preview for a specified preview token.
        /// </summary>
        /// <param name="previewToken">The preview token.</param>
        /// <remarks>
        /// <para>Tells the caches that they can dispose of any data that they would be keeping
        /// in order to provide preview to a given user. In the Xml cache this means deleting the Xml file,
        /// though other caches may do things differently.</para>
        /// <para>Does not handle the preview token storage (cookie, etc) that must be handled separately.</para>
        /// </remarks>
        void ExitPreview(string previewToken);

        #endregion

        #region Changes

        /* An IPublishedCachesService implementation can rely on transaction-level events to update
         * its internal, database-level data, as these events are purely internal. However, it cannot
         * rely on cache refreshers CacheUpdated events to update itself, as these events are external
         * and the order-of-execution of the handlers cannot be guaranteed, which means that some
         * user code may run before Umbraco is finished updating itself. Instead, the cache refreshers
         * explicitly notify the service of changes.
         *
         */

        /// <summary>
        /// Notifies of content cache refresher changes.
        /// </summary>
        /// <param name="payloads">The changes.</param>
        /// <param name="draftChanged">A value indicating whether draft contents have been changed in the cache.</param>
        /// <param name="publishedChanged">A value indicating whether published contents have been changed in the cache.</param>
        void Notify(ContentCacheRefresher.JsonPayload[] payloads, out bool draftChanged, out bool publishedChanged);

        /// <summary>
        /// Notifies of media cache refresher changes.
        /// </summary>
        /// <param name="payloads">The changes.</param>
        /// <param name="anythingChanged">A value indicating whether medias have been changed in the cache.</param>
        void Notify(MediaCacheRefresher.JsonPayload[] payloads, out bool anythingChanged);

        // there is no NotifyChanges for MemberCacheRefresher because we're not caching members.

        /// <summary>
        /// Notifies of content type refresher changes.
        /// </summary>
        /// <param name="payloads">The changes.</param>
        void Notify(ContentTypeCacheRefresher.JsonPayload[] payloads);

        /// <summary>
        /// Notifies of data type refresher changes.
        /// </summary>
        /// <param name="payloads">The changes.</param>
        void Notify(DataTypeCacheRefresher.JsonPayload[] payloads);

        /// <summary>
        /// Notifies of domain refresher changes.
        /// </summary>
        /// <param name="payloads">The changes.</param>
        void Notify(DomainCacheRefresher.JsonPayload[] payloads);

        #endregion
    }
}
