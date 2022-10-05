using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Models.Entities;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Services.Entities;

public class UserStartNodeEntitiesService : IUserStartNodeEntitiesService
{
    private readonly IEntityService _entityService;

    public UserStartNodeEntitiesService(IEntityService entityService) => _entityService = entityService;

    /// <inheritdoc />
    public IEnumerable<UserAccessEntity> RootUserAccessEntities(UmbracoObjectTypes umbracoObjectType, int[] userStartNodeIds)
    {
        // root entities for users without root access should include:
        // - the start nodes that are actual root entities (level == 1)
        // - the root level ancestors to the rest of the start nodes (required for browsing to the actual start nodes - will be marked as "no access")
        IEntitySlim[] userStartEntities = _entityService.GetAll(umbracoObjectType, userStartNodeIds).ToArray();

        // find the start nodes that are at root level (level == 1)
        IEntitySlim[] allowedTopmostEntities = userStartEntities.Where(entity => entity.Level == 1).ToArray();

        // find the root level ancestors of the rest of the start nodes, and add those as well
        var nonAllowedTopmostEntityIds = userStartEntities.Except(allowedTopmostEntities)
            .Select(entity => int.TryParse(entity.Path.Split(Constants.CharArrays.Comma).Skip(1).FirstOrDefault(), out var id) ? id : 0)
            .Where(id => id > 0)
            .ToArray();
        IEntitySlim[] nonAllowedTopmostEntities = nonAllowedTopmostEntityIds.Any()
            ? _entityService.GetAll(umbracoObjectType, nonAllowedTopmostEntityIds).ToArray()
            : Array.Empty<IEntitySlim>();

        return allowedTopmostEntities
            .Select(entity => new UserAccessEntity(entity, true))
            .Union(
                nonAllowedTopmostEntities
                    .Select(entity => new UserAccessEntity(entity, false)))
            .ToArray();
    }

    /// <inheritdoc />
    public IEnumerable<UserAccessEntity> ChildUserAccessEntities(IEnumerable<IEntitySlim> candidateChildren, string[] userStartNodePaths)
        // child entities for users without root access should include:
        // - children that are descendant-or-self of a user start node
        // - children that are ancestors of a user start node (required for browsing to the actual start nodes - will be marked as "no access")
        // all other candidate children should be discarded
        => candidateChildren.Select(child =>
        {
            // is descendant-or-self of a start node?
            if (IsDescendantOrSelf(child, userStartNodePaths))
            {
                return new UserAccessEntity(child, true);
            }

            // is ancestor of a start node?
            if (userStartNodePaths.Any(path => path.StartsWith(child.Path)))
            {
                return new UserAccessEntity(child, false);
            }

            return null;
        }).WhereNotNull().ToArray();

    /// <inheritdoc />
    public IEnumerable<UserAccessEntity> UserAccessEntities(IEnumerable<IEntitySlim> entities, string[] userStartNodePaths)
        // entities for users without root access should include:
        // - entities that are descendant-or-self of a user start node as regular entities
        // - all other entities as "no access" entities
        => entities.Select(entity => new UserAccessEntity(entity, IsDescendantOrSelf(entity, userStartNodePaths))).ToArray();

    private static bool IsDescendantOrSelf(IEntitySlim child, string[] userStartNodePaths)
        => userStartNodePaths.Any(path => child.Path.StartsWith(path));
}
