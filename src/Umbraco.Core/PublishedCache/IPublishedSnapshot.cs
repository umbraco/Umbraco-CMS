using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
///     Specifies a published snapshot.
/// </summary>
/// <remarks>
///     A published snapshot is a point-in-time capture of the current state of
///     everything that is "published".
/// </remarks>
public interface IPublishedSnapshot : IDisposable
{
    /// <summary>
    ///     Gets the <see cref="IPublishedContentCache" />.
    /// </summary>
    IPublishedContentCache? Content { get; }

    /// <summary>
    ///     Gets the <see cref="IPublishedMediaCache" />.
    /// </summary>
    IPublishedMediaCache? Media { get; }

    /// <summary>
    ///     Gets the <see cref="IPublishedMemberCache" />.
    /// </summary>
    IPublishedMemberCache? Members { get; }

    /// <summary>
    ///     Gets the <see cref="IDomainCache" />.
    /// </summary>
    IDomainCache? Domains { get; }

    /// <summary>
    ///     Gets the snapshot-level cache.
    /// </summary>
    /// <remarks>
    ///     <para>The snapshot-level cache belongs to this snapshot only.</para>
    /// </remarks>
    IAppCache? SnapshotCache { get; }

    /// <summary>
    ///     Gets the elements-level cache.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The elements-level cache is shared by all snapshots relying on the same elements,
    ///         ie all snapshots built on top of unchanging content / media / etc.
    ///     </para>
    /// </remarks>
    IAppCache? ElementsCache { get; }

    /// <summary>
    ///     Forces the preview mode.
    /// </summary>
    /// <param name="preview">The forced preview mode.</param>
    /// <param name="callback">A callback to execute when reverting to previous preview.</param>
    /// <remarks>
    ///     <para>
    ///         Forcing to false means no preview. Forcing to true means 'full' preview if the snapshot is not already
    ///         previewing;
    ///         otherwise the snapshot keeps previewing according to whatever settings it is using already.
    ///     </para>
    ///     <para>Stops forcing preview when disposed.</para>
    /// </remarks>
    IDisposable ForcedPreview(bool preview, Action<bool>? callback = null);
}
