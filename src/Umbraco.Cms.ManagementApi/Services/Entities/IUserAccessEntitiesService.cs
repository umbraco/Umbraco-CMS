using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.ManagementApi.Models.Entities;

namespace Umbraco.Cms.ManagementApi.Services.Entities;

public interface IUserAccessEntitiesService
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
    /// </remarks>
    IEnumerable<UserAccessEntity> RootEntities(UmbracoObjectTypes umbracoObjectType, int[] userStartNodeIds);

    /// <summary>
    /// Filters the applicable child entities for a list of candidate child entities.
    /// </summary>
    /// <param name="candidateChildren">The candidate child entities to filter (i.e. entities fetched with <see cref="EntityService.GetPagedChildren"/>).</param>
    /// <param name="userStartNodePaths">The calculated start node paths for the user.</param>
    /// <returns>A list of child entities applicable entities for the user.</returns>
    /// <remarks>
    /// The returned entities may include entities that outside of the user start node scope, but are needed to
    /// for browsing to the actual user start nodes. These entities will be marked as "no access" entities.
    /// </remarks>
    IEnumerable<UserAccessEntity> FilteredChildEntities(IEnumerable<IEntitySlim> candidateChildren, string[] userStartNodePaths);
}
