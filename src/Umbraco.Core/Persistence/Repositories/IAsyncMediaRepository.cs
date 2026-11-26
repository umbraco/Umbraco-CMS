using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Defines the repository contract for <see cref="IMedia" /> entities.
/// </summary>
public interface IAsyncMediaRepository : IAsyncContentRepository<IMedia>
{
    /// <summary>
    ///     Gets the media item whose file is stored at the given path.
    /// </summary>
    /// <remarks>
    ///     A path that names a resized rendition of the file, with a <c>_{width}x{height}</c> suffix before the
    ///     extension, resolves to the item storing the original file.
    /// </remarks>
    /// <param name="mediaPath">The path of the file, as stored by the file property of the media item.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The media item storing the file, or <c>null</c> if no media item stores a file at that path.</returns>
    Task<IMedia?> GetMediaByPathAsync(string mediaPath, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a paged list of media items at a given tree level, excluding trashed items.
    /// </summary>
    /// <param name="level">The tree level to filter by.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="ordering">The ordering specification. Must not be <c>null</c> — callers that don't have an opinion should use a service-layer facade that applies a default.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the matching, non-trashed media items.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="ordering" /> is <c>null</c>.</exception>
    Task<PagedModel<IMedia>> GetByLevelAsync(int level, int skip, int take, Ordering? ordering, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a paged list of ancestor media items of a media item, ordered root-first.
    /// </summary>
    /// <param name="key">The Guid key of the media item to retrieve ancestors for.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the media item's ancestors, root-first. Empty if the key does not resolve to a media item.</returns>
    Task<PagedModel<IMedia>> GetAncestorsAsync(Guid key, int skip, int take, CancellationToken cancellationToken);

    /// <summary>
    ///     Returns a value indicating whether the media recycle bin contains any media.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the recycle bin contains at least one media item; otherwise, <c>false</c>.</returns>
    Task<bool> RecycleBinSmellsAsync(CancellationToken cancellationToken);
}
