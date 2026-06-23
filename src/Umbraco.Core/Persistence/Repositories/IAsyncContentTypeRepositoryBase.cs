// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents an asynchronous base repository for content type composition entities.
/// </summary>
/// <remarks>
///     This is the asynchronous counterpart of <see cref="IContentTypeRepositoryBase{TItem}"/>. Both contracts
///     coexist for now; the synchronous one will be removed once the media- and member-type repositories have been
///     migrated to EF Core (out of scope for the document-type migration).
/// </remarks>
/// <typeparam name="TItem">The type of content type composition.</typeparam>
public interface IAsyncContentTypeRepositoryBase<TItem>
    : IAsyncReadWriteRepository<Guid, TItem>, IAsyncReadRepository<int, TItem>
    where TItem : IContentTypeComposition
{
    /// <summary>
    ///     Gets a content type by its alias.
    /// </summary>
    /// <param name="alias">The alias of the content type.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The content type if found; otherwise, <c>null</c>.</returns>
    Task<TItem?> GetAsync(string alias, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the content types that are direct children of the specified parent.
    /// </summary>
    /// <param name="parentId">The parent content type id.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The child content types, ordered by name.</returns>
    Task<IEnumerable<TItem>> GetByParentIdAsync(int parentId, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a value indicating whether the specified content type has any children.
    /// </summary>
    /// <param name="parentId">The parent content type id.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task<bool> HasChildrenAsync(int parentId, CancellationToken cancellationToken);

    /// <summary>
    ///     Counts all content types.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The total number of content types.</returns>
    Task<int> CountAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the content types that are allowed at the root.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The content types allowed at the root, ordered by name.</returns>
    Task<IEnumerable<TItem>> GetAllowedAsRootAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the element content types that are allowed in the library.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The element content types allowed in the library, ordered by name.</returns>
    Task<IEnumerable<TItem>> GetAllowedInLibraryAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Moves a content type to a container.
    /// </summary>
    /// <param name="moving">The content type to move.</param>
    /// <param name="container">The target container, or <c>null</c> to move to the root.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of move event information.</returns>
    Task<IEnumerable<MoveEventInfo<TItem>>> MoveAsync(TItem moving, EntityContainer? container, CancellationToken cancellationToken);

    /// <summary>
    ///     Derives a unique alias from an existing alias.
    /// </summary>
    /// <param name="alias">The original alias.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The original alias with a number appended to it, so that it is unique.</returns>
    /// <remarks>Unique across all content, media and member types.</remarks>
    Task<string> GetUniqueAliasAsync(string alias, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a value indicating whether there is a list view content item in the path.
    /// </summary>
    /// <param name="contentPath">The content path, as a comma-separated string of ids.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task<bool> HasContainerInPathAsync(string contentPath, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a value indicating whether there is a list view content item in the path.
    /// </summary>
    /// <param name="ids">The ids in the path.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task<bool> HasContainerInPathAsync(int[] ids, CancellationToken cancellationToken);

    /// <summary>
    ///     Returns a value indicating whether content nodes exist for the specified content type id.
    /// </summary>
    /// <param name="id">The content type id.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task<bool> HasContentNodesAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the allowed parent keys for a child content type.
    /// </summary>
    /// <param name="key">The child content type key.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The allowed parent keys.</returns>
    Task<IEnumerable<Guid>> GetAllowedParentKeysAsync(Guid key, CancellationToken cancellationToken);
}
