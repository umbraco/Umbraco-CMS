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
    ///     Retrieves a collection of nodes as navigation models for multiple object types.
    /// </summary>
    /// <param name="objectTypeKeys">The unique identifiers for the object types to include.</param>
    /// <returns>A collection of navigation models.</returns>
    IEnumerable<INavigationModel> GetContentNodesByObjectType(IEnumerable<Guid> objectTypeKeys);

    /// <summary>
    ///     Retrieves a collection of trashed content nodes as navigation models based on the object type key.
    /// </summary>
    /// <param name="objectTypeKey">The unique identifier for the object type.</param>
    /// <returns>A collection of navigation models.</returns>
    IEnumerable<INavigationModel> GetTrashedContentNodesByObjectType(Guid objectTypeKey);

    /// <summary>
    ///     Retrieves a collection of trashed nodes as navigation models for multiple object types.
    /// </summary>
    /// <param name="objectTypeKeys">The unique identifiers for the object types to include.</param>
    /// <returns>A collection of navigation models.</returns>
    IEnumerable<INavigationModel> GetTrashedContentNodesByObjectType(IEnumerable<Guid> objectTypeKeys);
}
