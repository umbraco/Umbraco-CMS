using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Services.Entities;

/// <summary>
/// Provides user start node filtering for tree controllers.
/// </summary>
/// <remarks>
/// Implementations resolve the current user's start node configuration and apply
/// access filtering to tree queries, ensuring users only see entities within their
/// permitted start nodes.
/// </remarks>
public interface IUserStartNodeTreeFilterService
{
    /// <summary>
    /// Determines whether start node filtering should be bypassed for the current user.
    /// </summary>
    /// <param name="dataTypeKey">An optional data type key; if the data type is configured to ignore user start nodes,
    /// filtering is bypassed.</param>
    /// <returns><c>true</c> if the user has root access or the data type ignores start nodes; otherwise, <c>false</c>.
    /// </returns>
    bool ShouldBypassStartNodeFiltering(Guid? dataTypeKey = null);

    /// <summary>
    /// Gets the root entities filtered by user start node access.
    /// </summary>
    /// <param name="totalItems">The total number of items returned.</param>
    /// <returns>An array of entities the user has access to at the root level, including ancestor "no access" entities
    /// for navigation.</returns>
    IEntitySlim[] GetFilteredRootEntities(out long totalItems);

    /// <summary>
    /// Gets the child entities of a parent filtered by user start node access.
    /// </summary>
    /// <param name="parentKey">The key of the parent entity.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    /// <param name="ordering">The ordering to apply.</param>
    /// <param name="totalItems">The total number of items available.</param>
    /// <returns>An array of child entities filtered by user access.</returns>
    IEntitySlim[] GetFilteredChildEntities(
        Guid parentKey,
        int skip,
        int take,
        Ordering ordering,
        out long totalItems);

    /// <summary>
    /// Gets the sibling entities of a target filtered by user start node access.
    /// </summary>
    /// <param name="target">The key of the target entity.</param>
    /// <param name="before">The number of siblings to retrieve before the target.</param>
    /// <param name="after">The number of siblings to retrieve after the target.</param>
    /// <param name="ordering">The ordering to apply.</param>
    /// <param name="totalBefore">The total number of siblings before the target.</param>
    /// <param name="totalAfter">The total number of siblings after the target.</param>
    /// <returns>An array of sibling entities filtered by user access.</returns>
    IEntitySlim[] GetFilteredSiblingEntities(
        Guid target,
        int before,
        int after,
        Ordering ordering,
        out long totalBefore,
        out long totalAfter);

    /// <summary>
    /// Maps entities to tree item view models, applying access filtering based on the user's start nodes.
    /// </summary>
    /// <typeparam name="TItem">The type of tree item view model.</typeparam>
    /// <param name="entities">The entities to map.</param>
    /// <param name="mapEntity">A function to map an entity the user has access to.</param>
    /// <param name="mapEntityAsNoAccess">A function to map an entity the user does not have direct access to (ancestor
    /// navigation items).</param>
    /// <returns>An array of mapped tree item view models, excluding entities not in the access map.</returns>
    TItem[] MapWithAccessFiltering<TItem>(
        IEntitySlim[] entities,
        Func<IEntitySlim, TItem> mapEntity,
        Func<IEntitySlim, TItem> mapEntityAsNoAccess)
        where TItem : class;
}
