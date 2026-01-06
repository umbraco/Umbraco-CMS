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

public class NotificationsRepository : INotificationsRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    public NotificationsRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

    private IScope? AmbientScope => _scopeAccessor.AmbientScope;

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
        Guid nodeType = AmbientScope.Database.FirstOrDefault<Guid>(sql);

        var dto = new User2NodeNotifyDto { Action = action, NodeId = entity.Id, UserId = user.Id };
        AmbientScope.Database.Insert(dto);
        notification = new Notification(dto.NodeId, dto.UserId, dto.Action, nodeType);
        return true;
    }
}
