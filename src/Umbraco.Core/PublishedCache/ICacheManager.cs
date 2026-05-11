using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
/// Provides unified access to all published content caches.
/// </summary>
/// <remarks>
/// The cache manager aggregates the content, media, member, and domain caches,
/// providing a single entry point for accessing cached published content.
/// It also provides access to the elements-level cache shared across snapshots.
/// </remarks>
public interface ICacheManager
{
    /// <summary>
    ///     Gets the <see cref="IPublishedContentCache" />.
    /// </summary>
    IPublishedContentCache Content { get; }

    /// <summary>
    ///     Gets the <see cref="IPublishedMediaCache" />.
    /// </summary>
    IPublishedMediaCache Media { get; }

    /// <summary>
    ///     Gets the <see cref="IPublishedMemberCache" />.
    /// </summary>
    IPublishedMemberCache Members { get; }

    /// <summary>
    ///     Gets the <see cref="IDomainCache" />.
    /// </summary>
    IDomainCache Domains { get; }

    /// <summary>
    ///     Gets the elements-level cache.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The elements-level cache is shared by all snapshots relying on the same elements,
    ///         ie all snapshots built on top of unchanging content / media / etc.
    ///     </para>
    /// </remarks>
    IAppCache ElementsCache { get; }
}
