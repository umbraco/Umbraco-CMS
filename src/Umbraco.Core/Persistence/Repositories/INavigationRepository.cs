using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for content navigation operations.
/// </summary>
public interface INavigationRepository
{
    /// <summary>
    ///     Retrieves a collection of content nodes as navigation models based on the object type key.
    /// </summary>
    /// <param name="objectTypeKey">The unique identifier for the object type.</param>
    /// <returns>A collection of navigation models.</returns>
    IEnumerable<INavigationModel> GetContentNodesByObjectType(Guid objectTypeKey);

    /// <summary>
    ///     Retrieves a collection of trashed content nodes as navigation models based on the object type key.
    /// </summary>
    /// <param name="objectTypeKey">The unique identifier for the object type.</param>
    /// <returns>A collection of navigation models.</returns>
    IEnumerable<INavigationModel> GetTrashedContentNodesByObjectType(Guid objectTypeKey);

    /// <summary>
    ///     Retrieves the content node with the given key together with all of its ancestors, ordered from the root down.
    /// </summary>
    /// <param name="key">The unique identifier of the node.</param>
    /// <param name="objectTypeKey">The unique identifier for the object type.</param>
    /// <returns>The navigation models of the ancestors and the node itself, or an empty collection when the node does not exist.</returns>
    // TODO (V19): Remove the default implementation.
    IEnumerable<INavigationModel> GetContentNodeWithAncestors(Guid key, Guid objectTypeKey) => [];
}
