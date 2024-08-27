using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Core.PublishedCache;

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
