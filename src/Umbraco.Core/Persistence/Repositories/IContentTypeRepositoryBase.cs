// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a base repository for content type composition entities.
/// </summary>
/// <typeparam name="TItem">The type of content type composition.</typeparam>
public interface IContentTypeRepositoryBase<TItem> : IReadWriteQueryRepository<int, TItem>, IReadRepository<Guid, TItem>
    where TItem : IContentTypeComposition
{
    /// <summary>
    ///     Gets a content type by its alias.
    /// </summary>
    /// <param name="alias">The alias of the content type.</param>
    /// <returns>The content type if found; otherwise, <c>null</c>.</returns>
    TItem? Get(string alias);

    /// <summary>
    ///     Moves a content type to a container.
    /// </summary>
    /// <param name="moving">The content type to move.</param>
    /// <param name="container">The target container.</param>
    /// <returns>A collection of move event information.</returns>
    IEnumerable<MoveEventInfo<TItem>> Move(TItem moving, EntityContainer container);

    /// <summary>
    ///     Derives a unique alias from an existing alias.
    /// </summary>
    /// <param name="alias">The original alias.</param>
    /// <returns>The original alias with a number appended to it, so that it is unique.</returns>
    /// <remarks>Unique across all content, media and member types.</remarks>
    string GetUniqueAlias(string alias);

    /// <summary>
    ///     Gets a value indicating whether there is a list view content item in the path.
    /// </summary>
    /// <param name="contentPath"></param>
    /// <returns></returns>
    bool HasContainerInPath(string contentPath);

    /// <summary>
    ///     Gets a value indicating whether there is a list view content item in the path.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    bool HasContainerInPath(params int[] ids);

    /// <summary>
    ///     Returns true or false depending on whether content nodes have been created based on the provided content type id.
    /// </summary>
    bool HasContentNodes(int id);
}
