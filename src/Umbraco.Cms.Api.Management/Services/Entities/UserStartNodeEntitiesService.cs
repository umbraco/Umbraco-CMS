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

/// <summary>
/// Provides functionality for retrieving user start node entities with access information.
/// </summary>
public class UserStartNodeEntitiesService : IUserStartNodeEntitiesService
{
    private readonly IEntityService _entityService;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserStartNodeEntitiesService"/> class.
    /// </summary>
    /// <param name="entityService">The entity service.</param>
    /// <param name="scopeProvider">The core scope provider.</param>
    /// <param name="idKeyMap">The ID to key mapping service.</param>
    public UserStartNodeEntitiesService(IEntityService entityService, ICoreScopeProvider scopeProvider, IIdKeyMap idKeyMap)
    {
        _entityService = entityService;
        _scopeProvider = scopeProvider;
        _idKeyMap = idKeyMap;
    }

    /// <inheritdoc />
    public IEnumerable<UserAccessEntity> RootUserAccessEntities(UmbracoObjectTypes umbracoObjectType, int[] userStartNodeIds)
    {
        // Root entities for users without root access should include:
        // - the start nodes that are actual root entities (level == 1)
        // - the root level ancestors to the rest of the start nodes (required for browsing to the actual start nodes - will be marked as "no access")
        IEntitySlim[] userStartEntities = userStartNodeIds.Any()
            ? _entityService.GetAll(umbracoObjectType, userStartNodeIds).ToArray()
            : Array.Empty<IEntitySlim>();

        // Find the start nodes that are at root level (level == 1).
        IEntitySlim[] allowedTopmostEntities = userStartEntities.Where(entity => entity.Level == 1).ToArray();

        // Find the root level ancestors of the rest of the start nodes, and add those as well.
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

    /// <inheritdoc/>
    public IEnumerable<UserAccessEntity> ChildUserAccessEntities(
        UmbracoObjectTypes umbracoObjectType,
        string[] userStartNodePaths,
        Guid parentKey,
        int skip,
        int take,
        Ordering ordering,
        out long totalItems)
    {
        Attempt<int> parentIdAttempt = _idKeyMap.GetIdForKey(parentKey, umbracoObjectType);
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
            // The requested parent is one of the user start nodes (or a descendant of one), all children are by definition allowed.
            children = _entityService.GetPagedChildren(parentKey, umbracoObjectType, skip, take, out totalItems, ordering: ordering).ToArray();
            return ChildUserAccessEntities(children, userStartNodePaths);
        }

        // Need to use a List here because the expression tree cannot convert an array when used in Contains.
        // See ExpressionTests.Sql_In().
        List<int> allowedChildIds = GetAllowedIds(userStartNodePaths, parentId);

        totalItems = allowedChildIds.Count;
        if (allowedChildIds.Count == 0)
        {
            // The requested parent is outside the scope of any user start nodes.
            return [];
        }

        // Even though we know the IDs of the allowed child entities to fetch, we still use a Query to yield correctly sorted children.
        IQuery<IUmbracoEntity> query = _scopeProvider.CreateQuery<IUmbracoEntity>().Where(x => allowedChildIds.Contains(x.Id));
        children = _entityService.GetPagedChildren(parentKey, umbracoObjectType, skip, take, out totalItems, query, ordering).ToArray();
        return ChildUserAccessEntities(children, userStartNodePaths);
    }

    private static List<int> GetAllowedIds(string[] userStartNodePaths, int parentId)
    {
        // If one or more of the user start nodes are descendants of the requested parent, find the "next child IDs" in those user start node paths
        // that are the final entries in the path.
        // E.g. given the user start node path "-1,2,3,4,5", if the requested parent ID is 3, the "next child ID" is 4.
        var userStartNodePathIds = userStartNodePaths.Select(path => path.Split(Constants.CharArrays.Comma).Select(int.Parse).ToArray()).ToArray();
        return userStartNodePathIds
            .Where(ids => ids.Contains(parentId))
            .Select(ids => ids[ids.IndexOf(parentId) + 1]) // Given the previous checks, the parent ID can never be the last in the user start node path, so this is safe
            .Distinct()
            .ToList();
    }

    /// <inheritdoc />
    public IEnumerable<UserAccessEntity> ChildUserAccessEntities(IEnumerable<IEntitySlim> candidateChildren, string[] userStartNodePaths)

        // Child or sibling entities for users without root access should include:
        // - children that are descendant-or-self of a user start node
        // - children that are ancestors of a user start node (required for browsing to the actual start nodes - will be marked as "no access")
        // All other candidate children should be discarded.
        => candidateChildren.Select(child =>
        {
            // is descendant-or-self of a start node?
            if (IsDescendantOrSelf(child, userStartNodePaths))
            {
                return new UserAccessEntity(child, true);
            }

            // is ancestor of a start node?
            // Note: Add trailing comma to prevent false matches (e.g., path "-1,100" should not match "-1,1001")
            if (userStartNodePaths.Any(path => path.StartsWith($"{child.Path},")))
            {
                return new UserAccessEntity(child, false);
            }

            return null;
        }).WhereNotNull().ToArray();

    /// <inheritdoc />
    public IEnumerable<UserAccessEntity> SiblingUserAccessEntities(
        UmbracoObjectTypes umbracoObjectType,
        string[] userStartNodePaths,
        Guid targetKey,
        int before,
        int after,
        Ordering ordering,
        out long totalBefore,
        out long totalAfter)
    {
        Attempt<int> targetIdAttempt = _idKeyMap.GetIdForKey(targetKey, umbracoObjectType);
        if (targetIdAttempt.Success is false)
        {
            totalBefore = 0;
            totalAfter = 0;
            return [];
        }

        var targetId = targetIdAttempt.Result;
        IEntitySlim? target = _entityService.Get(targetId);
        if (target is null)
        {
            totalBefore = 0;
            totalAfter = 0;
            return [];
        }

        IEntitySlim[] siblings;

        IEntitySlim? targetParent = _entityService.Get(target.ParentId);
        if (targetParent is null) // Even if the parent is the root, we still expect to get a value here.
        {
            totalBefore = 0;
            totalAfter = 0;
            return [];
        }

        if (userStartNodePaths.Any(path => $"{targetParent?.Path},".StartsWith($"{path},")))
        {
            // The requested parent of the target is one of the user start nodes (or a descendant of one), all siblings are by definition allowed.
            siblings = _entityService.GetSiblings(targetKey, [umbracoObjectType], before, after, out totalBefore, out totalAfter, ordering: ordering).ToArray();
            return ChildUserAccessEntities(siblings, userStartNodePaths);
        }

        List<int> allowedSiblingIds = GetAllowedIds(userStartNodePaths, targetParent.Id);

        if (allowedSiblingIds.Count == 0)
        {
            // The requested target is outside the scope of any user start nodes.
            totalBefore = 0;
            totalAfter = 0;
            return [];
        }

        // Even though we know the IDs of the allowed sibling entities to fetch, we still use a Query to yield correctly sorted children.
        IQuery<IUmbracoEntity> query = _scopeProvider.CreateQuery<IUmbracoEntity>().Where(x => allowedSiblingIds.Contains(x.Id));
        siblings = _entityService.GetSiblings(targetKey, [umbracoObjectType], before, after, out totalBefore, out totalAfter, query, ordering).ToArray();
        return ChildUserAccessEntities(siblings, userStartNodePaths);
    }

    /// <inheritdoc />
    public IEnumerable<UserAccessEntity> UserAccessEntities(IEnumerable<IEntitySlim> entities, string[] userStartNodePaths)

        // Entities for users without root access should include:
        // - entities that are descendant-or-self of a user start node as regular entities
        // - all other entities as "no access" entities
        => entities.Select(entity => new UserAccessEntity(entity, IsDescendantOrSelf(entity, userStartNodePaths))).ToArray();

    private static bool IsDescendantOrSelf(IEntitySlim child, string[] userStartNodePaths)
        // Note: Add trailing commas to both paths to prevent false matches (e.g., path "-1,100" should not match "-1,1001")
        // This matches the pattern used in lines 92 and 192 of this file
        => userStartNodePaths.Any(path => $"{child.Path},".StartsWith($"{path},"));
}
