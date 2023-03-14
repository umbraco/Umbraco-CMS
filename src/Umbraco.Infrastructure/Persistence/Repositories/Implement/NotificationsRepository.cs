using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

public class NotificationsRepository : INotificationsRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    public NotificationsRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

    private IScope? AmbientScope => _scopeAccessor.AmbientScope;

    public IEnumerable<Notification>? GetUsersNotifications(IEnumerable<int> userIds, string? action, IEnumerable<int> nodeIds, Guid objectType)
    {
        var nodeIdsA = nodeIds.ToArray();
        Sql<ISqlContext>? sql = AmbientScope?.SqlContext.Sql()
            .Select(
                "DISTINCT umbracoNode.id nodeId, umbracoUser.id userId, umbracoNode.nodeObjectType, umbracoUser2NodeNotify.action")
            .From<User2NodeNotifyDto>()
            .InnerJoin<NodeDto>().On<User2NodeNotifyDto, NodeDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<UserDto>().On<User2NodeNotifyDto, UserDto>(left => left.UserId, right => right.Id)
            .Where<NodeDto>(x => x.NodeObjectType == objectType)
            .Where<UserDto>(x => x.Disabled == false) // only approved users
            .Where<User2NodeNotifyDto>(x => x.Action == action); // on the specified action
        if (nodeIdsA.Length > 0)
        {
            sql?
                .WhereIn<NodeDto>(x => x.NodeId, nodeIdsA); // for the specified nodes
        }

        sql?
            .OrderBy<UserDto>(x => x.Id)
            .OrderBy<NodeDto>(dto => dto.NodeId);
        return AmbientScope?.Database.Fetch<UserNotificationDto>(sql)
            .Select(x => new Notification(x.NodeId, x.UserId, x.Action, objectType));
    }

    public IEnumerable<Notification>? GetUserNotifications(IUser user)
    {
        Sql<ISqlContext>? sql = AmbientScope?.SqlContext.Sql()
            .Select(
                "DISTINCT umbracoNode.id AS nodeId, umbracoUser2NodeNotify.userId, umbracoNode.nodeObjectType, umbracoUser2NodeNotify.action")
            .From<User2NodeNotifyDto>()
            .InnerJoin<NodeDto>()
            .On<User2NodeNotifyDto, NodeDto>(dto => dto.NodeId, dto => dto.NodeId)
            .Where<User2NodeNotifyDto>(dto => dto.UserId == user.Id)
            .OrderBy<NodeDto>(dto => dto.NodeId);

        List<UserNotificationDto>? dtos = AmbientScope?.Database.Fetch<UserNotificationDto>(sql);

        // need to map the results
        return dtos?.Select(d => new Notification(d.NodeId, d.UserId, d.Action, d.NodeObjectType)).ToList();
    }

    public IEnumerable<Notification> SetNotifications(IUser user, IEntity entity, string[] actions)
    {
        DeleteNotifications(user, entity);
        return actions.Select(action => CreateNotification(user, entity, action)).ToList();
    }

    public IEnumerable<Notification>? GetEntityNotifications(IEntity entity)
    {
        Sql<ISqlContext>? sql = AmbientScope?.SqlContext.Sql()
            .Select(
                "DISTINCT umbracoNode.id as nodeId, umbracoUser2NodeNotify.userId, umbracoNode.nodeObjectType, umbracoUser2NodeNotify.action")
            .From<User2NodeNotifyDto>()
            .InnerJoin<NodeDto>()
            .On<User2NodeNotifyDto, NodeDto>(dto => dto.NodeId, dto => dto.NodeId)
            .Where<User2NodeNotifyDto>(dto => dto.NodeId == entity.Id)
            .OrderBy<NodeDto>(dto => dto.NodeId);

        List<UserNotificationDto>? dtos = AmbientScope?.Database.Fetch<UserNotificationDto>(sql);

        // need to map the results
        return dtos?.Select(d => new Notification(d.NodeId, d.UserId, d.Action, d.NodeObjectType)).ToList();
    }

    public int DeleteNotifications(IEntity entity) =>
        AmbientScope?.Database.Delete<User2NodeNotifyDto>("WHERE nodeId = @nodeId", new { nodeId = entity.Id }) ?? 0;

    public int DeleteNotifications(IUser user) =>
        AmbientScope?.Database.Delete<User2NodeNotifyDto>("WHERE userId = @userId", new { userId = user.Id }) ?? 0;

    public int DeleteNotifications(IUser user, IEntity entity) =>

        // delete all settings on the node for this user
        AmbientScope?.Database.Delete<User2NodeNotifyDto>(
            "WHERE userId = @userId AND nodeId = @nodeId",
            new { userId = user.Id, nodeId = entity.Id }) ?? 0;

    public Notification CreateNotification(IUser user, IEntity entity, string action)
    {
        Sql<ISqlContext>? sql = AmbientScope?.SqlContext.Sql()
            .Select("DISTINCT nodeObjectType")
            .From<NodeDto>()
            .Where<NodeDto>(nodeDto => nodeDto.NodeId == entity.Id);
        Guid? nodeType = AmbientScope?.Database.ExecuteScalar<Guid>(sql);

        var dto = new User2NodeNotifyDto { Action = action, NodeId = entity.Id, UserId = user.Id };
        AmbientScope?.Database.Insert(dto);
        return new Notification(dto.NodeId, dto.UserId, dto.Action, nodeType);
    }
}
