using System.Diagnostics.CodeAnalysis;
using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
/// Provides methods for managing notification entities in the persistence layer of Umbraco CMS.
/// </summary>
public class NotificationsRepository : INotificationsRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationsRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">
    /// The <see cref="IScopeAccessor"/> used to manage the database scope for repository operations.
    /// </param>
    public NotificationsRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

    private IScope? AmbientScope => _scopeAccessor.AmbientScope;

    /// <summary>
    /// Gets notifications for the specified users filtered by action, node IDs, and object type.
    /// </summary>
    /// <param name="userIds">The collection of user IDs to retrieve notifications for.</param>
    /// <param name="action">The action to filter notifications by; can be null.</param>
    /// <param name="nodeIds">The collection of node IDs to filter notifications by.</param>
    /// <param name="objectType">The object type GUID to filter notifications by.</param>
    /// <returns>An enumerable of <see cref="Notification"/> objects matching the specified criteria.</returns>
    public IEnumerable<Notification> GetUsersNotifications(IEnumerable<int> userIds, string? action, IEnumerable<int> nodeIds, Guid objectType)
    {
        if (AmbientScope is null)
        {
            return [];
        }

        var nodeIdsA = nodeIds.ToArray();


        ISqlSyntaxProvider syntax = AmbientScope.SqlContext.SqlSyntax;
        Sql<ISqlContext> sql = AmbientScope.SqlContext.Sql()
            .Select($@"DISTINCT
{syntax.GetQuotedTableName("umbracoNode")}.id {syntax.GetQuotedName("nodeId")},
{syntax.GetQuotedTableName("umbracoUser")}.id {syntax.GetQuotedName("userId")},
{syntax.GetQuotedTableName("umbracoNode")}.{syntax.GetQuotedColumnName("nodeObjectType")},
{syntax.GetQuotedTableName("umbracoUser2NodeNotify")}.action")
            .From<User2NodeNotifyDto>()
            .InnerJoin<NodeDto>().On<User2NodeNotifyDto, NodeDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<UserDto>().On<User2NodeNotifyDto, UserDto>(left => left.UserId, right => right.Id)
            .Where<NodeDto>(x => x.NodeObjectType == objectType)
            .Where<UserDto>(x => x.Disabled == false) // only approved users
            .Where<User2NodeNotifyDto>(x => x.Action == action); // on the specified action
        if (nodeIdsA.Length > 0)
        {
            sql.WhereIn<NodeDto>(x => x.NodeId, nodeIdsA); // for the specified nodes
        }

        sql
            .OrderBy<UserDto>(x => x.Id)
            .OrderBy<NodeDto>(dto => dto.NodeId);
        return AmbientScope.Database.Fetch<UserNotificationDto>(sql)
            .Select(x => new Notification(x.NodeId, x.UserId, x.Action, objectType));
    }

    /// <summary>
    /// Retrieves the notifications associated with content nodes for the specified user.
    /// </summary>
    /// <param name="user">The user whose content node notifications are to be retrieved.</param>
    /// <returns>
    /// An enumerable collection of <see cref="Notification"/> objects representing the user's notifications for content nodes.
    /// </returns>
    public IEnumerable<Notification> GetUserNotifications(IUser user)
    {
        if (AmbientScope is null)
        {
            return [];
        }

        ISqlSyntaxProvider syntax = AmbientScope.SqlContext.SqlSyntax;
        Sql<ISqlContext> sql = AmbientScope.SqlContext.Sql()
            .Select($@"DISTINCT
{syntax.GetQuotedTableName("umbracoNode")}.id {syntax.GetQuotedName("nodeId")},
{syntax.GetQuotedTableName("umbracoUser2NodeNotify")}.{syntax.GetQuotedColumnName("userId")},
{syntax.GetQuotedTableName("umbracoNode")}.{syntax.GetQuotedColumnName("nodeObjectType")},
{syntax.GetQuotedTableName("umbracoUser2NodeNotify")}.action")
            .From<User2NodeNotifyDto>()
            .InnerJoin<NodeDto>()
            .On<User2NodeNotifyDto, NodeDto>(dto => dto.NodeId, dto => dto.NodeId)
            .Where<User2NodeNotifyDto>(dto => dto.UserId == user.Id)
            .OrderBy<NodeDto>(dto => dto.NodeId);

        List<UserNotificationDto> dtos = AmbientScope.Database.Fetch<UserNotificationDto>(sql);

        // need to map the results
        return dtos.Select(d => new Notification(d.NodeId, d.UserId, d.Action, d.NodeObjectType)).ToList();
    }

    /// <summary>
    /// Sets notifications for the specified user and entity by replacing any existing notifications with new ones for the given actions.
    /// </summary>
    /// <param name="user">The user for whom to set notifications.</param>
    /// <param name="entity">The entity associated with the notifications.</param>
    /// <param name="actions">An array of action names for which to create notifications.</param>
    /// <returns>An enumerable of the created <see cref="Notification"/> objects.</returns>
    public IEnumerable<Notification> SetNotifications(IUser user, IEntity entity, string[] actions)
    {
        DeleteNotifications(user, entity);
        return actions
            .Select(action =>
            {
                TryCreateNotification(user, entity, action, out Notification? n);
                return n;
            })
            .WhereNotNull()
            .ToList();
    }

    /// <summary>
    /// Retrieves all user notifications associated with the specified entity.
    /// </summary>
    /// <param name="entity">The entity for which to retrieve user notifications.</param>
    /// <returns>
    /// An <see cref="IEnumerable{Notification}"/> containing all notifications related to the given entity, or an empty collection if none exist.
    /// </returns>
    public IEnumerable<Notification> GetEntityNotifications(IEntity entity)
    {
        if (AmbientScope is null)
        {
            return [];
        }

        ISqlSyntaxProvider syntax = AmbientScope.SqlContext.SqlSyntax;
        Sql<ISqlContext> sql = AmbientScope.SqlContext.Sql()
            .Select($@"DISTINCT
{syntax.GetQuotedTableName("umbracoNode")}.id {syntax.GetQuotedName("nodeId")},
{syntax.GetQuotedTableName("umbracoUser2NodeNotify")}.{syntax.GetQuotedColumnName("userId")},
{syntax.GetQuotedTableName("umbracoNode")}.{syntax.GetQuotedColumnName("nodeObjectType")},
{syntax.GetQuotedTableName("umbracoUser2NodeNotify")}.action")
            .From<User2NodeNotifyDto>()
            .InnerJoin<NodeDto>()
            .On<User2NodeNotifyDto, NodeDto>(dto => dto.NodeId, dto => dto.NodeId)
            .Where<User2NodeNotifyDto>(dto => dto.NodeId == entity.Id)
            .OrderBy<NodeDto>(dto => dto.NodeId);

        List<UserNotificationDto> dtos = AmbientScope.Database.Fetch<UserNotificationDto>(sql);

        // need to map the results
        return dtos.Select(d => new Notification(d.NodeId, d.UserId, d.Action, d.NodeObjectType)).ToList();
    }

    /// <summary>
    /// Deletes all notifications associated with the specified entity.
    /// </summary>
    /// <param name="entity">The entity for which to delete notifications.</param>
    /// <returns>The number of notifications that were deleted.</returns>
    public int DeleteNotifications(IEntity entity)
    {
        if (AmbientScope == null)
        {
            return 0;
        }

        Sql<ISqlContext> sql = AmbientScope.SqlContext.Sql()
            .Delete<User2NodeNotifyDto>()
            .Where<User2NodeNotifyDto>(dto => dto.NodeId == entity.Id);
        return AmbientScope.Database.Execute(sql);
    }

    /// <summary>
    /// Deletes all notifications associated with the specified user.
    /// </summary>
    /// <param name="user">The <see cref="IUser"/> for which to delete notifications.</param>
    /// <returns>The number of notifications that were deleted.</returns>
    public int DeleteNotifications(IUser user)
    {
        if (AmbientScope == null)
        {
            return 0;
        }

        Sql<ISqlContext> sql = AmbientScope.SqlContext.Sql()
            .Delete<User2NodeNotifyDto>()
            .Where<User2NodeNotifyDto>(dto => dto.UserId == user.Id);
        return AmbientScope.Database.Execute(sql);
    }

    public int DeleteNotifications(IUser user, IEntity entity)
    {
        if (AmbientScope == null)
        {
            return 0;
        }

        // delete all settings on the node for this user
        Sql<ISqlContext> sql = AmbientScope.SqlContext.Sql()
            .Delete<User2NodeNotifyDto>()
            .Where<User2NodeNotifyDto>(dto => dto.NodeId == entity.Id && dto.UserId == user.Id);
        return AmbientScope.Database.Execute(sql);
    }

    /// <summary>
    /// Attempts to create a notification for a specified user and entity based on a given action.
    /// </summary>
    /// <param name="user">The user who will receive the notification.</param>
    /// <param name="entity">The entity (such as a content item or node) associated with the notification.</param>
    /// <param name="action">A string representing the action that triggers the notification (e.g., "save", "publish").</param>
    /// <param name="notification">When this method returns, contains the created <see cref="Notification"/> if successful; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the notification was successfully created and assigned to <paramref name="notification"/>; otherwise, <c>false</c>.</returns>
    public bool TryCreateNotification(IUser user, IEntity entity, string action, [NotNullWhen(true)] out Notification? notification)
    {
        if (AmbientScope is null)
        {
            notification = null;
            return false;
        }

        Sql<ISqlContext>? sql = AmbientScope.SqlContext.Sql()
            .SelectDistinct<NodeDto>(c => c.NodeObjectType)
            .From<NodeDto>()
            .Where<NodeDto>(nodeDto => nodeDto.NodeId == entity.Id);

        // We must use FirstOrDefault over ExecuteScalar when retrieving a Guid, to ensure we go through the full NPoco mapping pipeline.
        // Without that, though it will succeed on SQLite and SQLServer, it could fail on other database providers.
        Guid nodeType = AmbientScope.Database.FirstOrDefault<Guid>(sql);

        var dto = new User2NodeNotifyDto { Action = action, NodeId = entity.Id, UserId = user.Id };
        AmbientScope.Database.Insert(dto);
        notification = new Notification(dto.NodeId, dto.UserId, dto.Action, nodeType);
        return true;
    }
}
