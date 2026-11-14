using Umbraco.Cms.Api.Management.Models.Entities;
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
    /// <returns>An array of user access entities at the root level, each indicating whether the user has direct access
    /// or if the entity is an ancestor navigation item.</returns>
    UserAccessEntity[] GetFilteredRootEntities(out long totalItems);

    /// <summary>
    /// Gets the child entities of a parent filtered by user start node access.
    /// </summary>
    /// <param name="parentKey">The key of the parent entity.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    /// <param name="ordering">The ordering to apply.</param>
    /// <param name="totalItems">The total number of items available.</param>
    /// <returns>An array of child user access entities filtered by user start node access.</returns>
    UserAccessEntity[] GetFilteredChildEntities(
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
    /// <returns>An array of sibling user access entities filtered by user start node access.</returns>
    UserAccessEntity[] GetFilteredSiblingEntities(
        Guid target,
        int before,
        int after,
        Ordering ordering,
        out long totalBefore,
        out long totalAfter);

    /// <summary>
    /// Filters the provided entities by user start node access, keeping only those the tree would show.
    /// </summary>
    /// <param name="entities">The entities to filter.</param>
    /// <returns>
    /// The entities at or below a user start node, which the user has access to, and those above one, which the user
    /// can only navigate through. Entities in neither position are omitted.
    /// </returns>
    /// <remarks>
    /// The default implementation resolves each entity on its own through
    /// <see cref="GetFilteredSiblingEntities"/>, so an implementation that does not override this method applies its
    /// existing access decisions, at the cost of a lookup per entity.
    /// </remarks>
    // TODO (V20): Remove the default implementation.
    UserAccessEntity[] GetFilteredEntities(IEnumerable<IEntitySlim> entities)
        => entities
            .SelectMany(entity => GetFilteredSiblingEntities(
                    entity.Key,
                    0,
                    0,
                    Ordering.By(Infrastructure.Persistence.Dtos.NodeDto.SortOrderColumnName),
                    out _,
                    out _)
                .Where(userAccessEntity => userAccessEntity.Entity.Key == entity.Key))
            .ToArray();

    /// <summary>
    /// Maps entities to tree item view models, applying access filtering using the provided access map.
    /// </summary>
    /// <typeparam name="TItem">The type of tree item view model.</typeparam>
    /// <param name="entities">The entities to map.</param>
    /// <param name="accessMap">A dictionary mapping entity keys to their access status, as obtained from a prior call
    /// to one of the <c>GetFiltered*Entities</c> methods.</param>
    /// <param name="mapEntity">A function to map an entity the user has access to.</param>
    /// <param name="mapEntityAsNoAccess">A function to map an entity the user does not have direct access to (ancestor
    /// navigation items).</param>
    /// <returns>An array of mapped tree item view models, excluding entities not in the access map.</returns>
    TItem[] MapWithAccessFiltering<TItem>(
        IEntitySlim[] entities,
        Dictionary<Guid, bool> accessMap,
        Func<IEntitySlim, TItem> mapEntity,
        Func<IEntitySlim, TItem> mapEntityAsNoAccess)
        where TItem : class;
}
