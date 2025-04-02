using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Models.Entities;

namespace Umbraco.Cms.Api.Management.Services.Entities;

public interface IUserStartNodeEntitiesService
{
    /// <summary>
    /// Calculates the applicable root entities for a given object type for users without root access.
    /// </summary>
    /// <param name="umbracoObjectType">The object type.</param>
    /// <param name="userStartNodeIds">The calculated start node IDs for the user.</param>
    /// <returns>A list of root entities for the user.</returns>
    /// <remarks>
    /// The returned entities may include entities that outside of the user start node scope, but are needed to
    /// for browsing to the actual user start nodes. These entities will be marked as "no access" entities.
    ///
    /// This method does not support pagination, because it must load all entities explicitly in order to calculate
    /// the correct result, given that user start nodes can be descendants of root nodes. Consumers need to apply
    /// pagination to the result if applicable.
    /// </remarks>
    IEnumerable<UserAccessEntity> RootUserAccessEntities(UmbracoObjectTypes umbracoObjectType, int[] userStartNodeIds);

    /// <summary>
    /// Calculates the applicable child entities for a given object type for users without root access.
    /// </summary>
    /// <param name="umbracoObjectType">The object type.</param>
    /// <param name="userStartNodePaths">The calculated start node paths for the user.</param>
    /// <param name="parentKey">The key of the parent.</param>
    /// <param name="skip">The number of applicable children to skip.</param>
    /// <param name="take">The number of applicable children to take.</param>
    /// <param name="ordering">The ordering to apply when fetching and paginating the children.</param>
    /// <param name="totalItems">The total number of applicable children available.</param>
    /// <returns>A list of child entities applicable for the user.</returns>
    /// <remarks>
    /// The returned entities may include entities that outside of the user start node scope, but are needed to
    /// for browsing to the actual user start nodes. These entities will be marked as "no access" entities.
    /// </remarks>
    IEnumerable<UserAccessEntity> ChildUserAccessEntities(
        UmbracoObjectTypes umbracoObjectType,
        string[] userStartNodePaths,
        Guid parentKey,
        int skip,
        int take,
        Ordering ordering,
        out long totalItems)
    {
        totalItems = 0;
        return [];
    }

    /// <summary>
    /// Calculates the applicable child entities from a list of candidate child entities for users without root access.
    /// </summary>
    /// <param name="candidateChildren">The candidate child entities to filter (i.e. entities fetched with <see cref="EntityService.GetPagedChildren"/>).</param>
    /// <param name="userStartNodePaths">The calculated start node paths for the user.</param>
    /// <returns>A list of child entities applicable entities for the user.</returns>
    /// <remarks>
    /// The returned entities may include entities that outside of the user start node scope, but are needed to
    /// for browsing to the actual user start nodes. These entities will be marked as "no access" entities.
    /// Some candidate entities may be filtered out if they are not applicable for the user scope.
    /// </remarks>
    IEnumerable<UserAccessEntity> ChildUserAccessEntities(IEnumerable<IEntitySlim> candidateChildren, string[] userStartNodePaths);

    /// <summary>
    /// Calculates the access level of a collection of entities for users without root access.
    /// </summary>
    /// <param name="entities">The entities.</param>
    /// <param name="userStartNodePaths">The calculated start node paths for the user.</param>
    /// <returns> The access level for each entity.</returns>
    IEnumerable<UserAccessEntity> UserAccessEntities(IEnumerable<IEntitySlim> entities, string[] userStartNodePaths);
}
