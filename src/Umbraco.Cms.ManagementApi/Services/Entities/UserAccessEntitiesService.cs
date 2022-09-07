using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Models.Entities;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Services.Entities;

public class UserAccessEntitiesService : IUserAccessEntitiesService
{
    private readonly IEntityService _entityService;

    public UserAccessEntitiesService(IEntityService entityService) => _entityService = entityService;

    /// <inheritdoc />
    public IEnumerable<UserAccessEntity> RootEntities(UmbracoObjectTypes umbracoObjectType, int[] userStartNodeIds)
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
    public IEnumerable<UserAccessEntity> FilteredChildEntities(IEnumerable<IEntitySlim> candidateChildren, string[] userStartNodePaths)
        // child items for users without root access should include:
        // - children that are descendant-or-self of a user start node
        // - children that are ancestors of a user start node (required for browsing to the actual start nodes - will be marked as "no access")
        => candidateChildren.Select(child =>
        {
            // is descendant-or-self of a start node?
            if (userStartNodePaths.Any(path => child.Path.StartsWith(path)))
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
}
