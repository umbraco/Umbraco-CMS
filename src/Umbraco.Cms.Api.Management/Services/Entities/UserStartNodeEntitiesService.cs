using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Models.Entities;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Services.Entities;

public class UserStartNodeEntitiesService : IUserStartNodeEntitiesService
{
    private readonly IEntityService _entityService;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IIdKeyMap _idKeyMap;

    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in V17.")]
    public UserStartNodeEntitiesService(IEntityService entityService)
        : this(
            entityService,
            StaticServiceProvider.Instance.GetRequiredService<ICoreScopeProvider>(),
            StaticServiceProvider.Instance.GetRequiredService<IIdKeyMap>())
    {
    }

    public UserStartNodeEntitiesService(IEntityService entityService, ICoreScopeProvider scopeProvider, IIdKeyMap idKeyMap)
    {
        _entityService = entityService;
        _scopeProvider = scopeProvider;
        _idKeyMap = idKeyMap;
    }

    /// <inheritdoc />
    public IEnumerable<UserAccessEntity> RootUserAccessEntities(UmbracoObjectTypes umbracoObjectType, int[] userStartNodeIds)
    {
        // root entities for users without root access should include:
        // - the start nodes that are actual root entities (level == 1)
        // - the root level ancestors to the rest of the start nodes (required for browsing to the actual start nodes - will be marked as "no access")
        IEntitySlim[] userStartEntities = userStartNodeIds.Any()
            ? _entityService.GetAll(umbracoObjectType, userStartNodeIds).ToArray()
            : Array.Empty<IEntitySlim>();

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

    public IEnumerable<UserAccessEntity> ChildUserAccessEntities(UmbracoObjectTypes umbracoObjectType, string[] userStartNodePaths, Guid parentKey, int skip, int take, Ordering ordering, out long totalItems)
    {
        Attempt<int> parentIdAttempt = _idKeyMap.GetIdForKey(parentKey, UmbracoObjectTypes.Document);
        if (parentIdAttempt.Success is false)
        {
            totalItems = 0;
            return [];
        }

        var parentId = parentIdAttempt.Result;
        IEntitySlim? parent = _entityService.Get(parentId);
        if (parent is null)
        {
            totalItems = 0;
            return [];
        }

        IEntitySlim[] children;
        if (userStartNodePaths.Any(path => $"{parent.Path},".StartsWith($"{path},")))
        {
            // the requested parent is one of the user start nodes (or a descendant of one), all children are by definition allowed
            children = _entityService.GetPagedChildren(parentKey, umbracoObjectType, skip, take, out totalItems, ordering: ordering).ToArray();
            return ChildUserAccessEntities(children, userStartNodePaths);
        }

        // if one or more of the user start nodes are descendants of the requested parent, find the "next child IDs" in those user start node paths
        // - e.g. given the user start node path "-1,2,3,4,5", if the requested parent ID is 3, the "next child ID" is 4.
        var userStarNodePathIds = userStartNodePaths.Select(path => path.Split(Constants.CharArrays.Comma).Select(int.Parse).ToArray()).ToArray();
        var allowedChildIds = userStarNodePathIds
            .Where(ids => ids.Contains(parentId))
            // given the previous checks, the parent ID can never be the last in the user start node path, so this is safe
            .Select(ids => ids[ids.IndexOf(parentId) + 1])
            .Distinct()
            .ToArray();

        totalItems = allowedChildIds.Length;
        if (allowedChildIds.Length == 0)
        {
            // the requested parent is outside the scope of any user start nodes
            return [];
        }

        // even though we know the IDs of the allowed child entities to fetch, we still use a Query to yield correctly sorted children
        IQuery<IUmbracoEntity> query = _scopeProvider.CreateQuery<IUmbracoEntity>().Where(x => allowedChildIds.Contains(x.Id));
        children = _entityService.GetPagedChildren(parentKey, umbracoObjectType, skip, take, out totalItems, query, ordering).ToArray();
        return ChildUserAccessEntities(children, userStartNodePaths);
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
