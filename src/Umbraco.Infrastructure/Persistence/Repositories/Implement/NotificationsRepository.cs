using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement
{
    public class NotificationsRepository : INotificationsRepository
    {
        public IEnumerable<Notification> GetUsersNotifications(IScope scope, IEnumerable<int> userIds, string action, IEnumerable<int> nodeIds, Guid objectType)
        {
            var nodeIdsA = nodeIds.ToArray();
            Sql<ISqlContext> sql = scope.SqlContext.Sql()
                    .Select("DISTINCT umbracoNode.id nodeId, umbracoUser.id userId, umbracoNode.nodeObjectType, umbracoUser2NodeNotify.action")
                    .From<User2NodeNotifyDto>()
                    .InnerJoin<NodeDto>().On<User2NodeNotifyDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                    .InnerJoin<UserDto>().On<User2NodeNotifyDto, UserDto>(left => left.UserId, right => right.Id)
                    .Where<NodeDto>(x => x.NodeObjectType == objectType)
                    .Where<UserDto>(x => x.Disabled == false) // only approved users
                    .Where<User2NodeNotifyDto>(x => x.Action == action); // on the specified action
            if (nodeIdsA.Length > 0)
            {
                sql
                    .WhereIn<NodeDto>(x => x.NodeId, nodeIdsA); // for the specified nodes
            }

            sql
                .OrderBy<UserDto>(x => x.Id)
                .OrderBy<NodeDto>(dto => dto.NodeId);
            return scope.Database.Fetch<dynamic>(sql).Select(x => new Notification(x.nodeId, x.userId, x.action, objectType));
        }

        public IEnumerable<Notification> GetUserNotifications(IScope scope, IUser user)
        {
            Sql<ISqlContext> sql = scope.SqlContext.Sql()
                .Select("DISTINCT umbracoNode.id, umbracoUser2NodeNotify.userId, umbracoNode.nodeObjectType, umbracoUser2NodeNotify.action")
                .From<User2NodeNotifyDto>()
                .InnerJoin<NodeDto>()
                .On<User2NodeNotifyDto, NodeDto>(dto => dto.NodeId, dto => dto.NodeId)
                .Where<User2NodeNotifyDto>(dto => dto.UserId == (int)user.Id)
                .OrderBy<NodeDto>(dto => dto.NodeId);

            List<dynamic> dtos = scope.Database.Fetch<dynamic>(sql);

            // Need to map the results
            return dtos.Select(d => new Notification(d.id, d.userId, d.action, d.nodeObjectType)).ToList();
        }

        public IEnumerable<Notification> SetNotifications(IScope scope, IUser user, IEntity entity, string[] actions)
        {
            DeleteNotifications(scope, user, entity);
            return actions.Select(action => CreateNotification(scope, user, entity, action)).ToList();
        }

        public IEnumerable<Notification> GetEntityNotifications(IScope scope, IEntity entity)
        {
            Sql<ISqlContext> sql = scope.SqlContext.Sql()
                .Select("DISTINCT umbracoNode.id, umbracoUser2NodeNotify.userId, umbracoNode.nodeObjectType, umbracoUser2NodeNotify.action")
                .From<User2NodeNotifyDto>()
                .InnerJoin<NodeDto>()
                .On<User2NodeNotifyDto, NodeDto>(dto => dto.NodeId, dto => dto.NodeId)
                .Where<User2NodeNotifyDto>(dto => dto.NodeId == entity.Id)
                .OrderBy<NodeDto>(dto => dto.NodeId);

            List<dynamic> dtos = scope.Database.Fetch<dynamic>(sql);

            // Need to map the results
            return dtos.Select(d => new Notification(d.id, d.userId, d.action, d.nodeObjectType)).ToList();
        }

        public int DeleteNotifications(IScope scope, IEntity entity) =>
            scope.Database.Delete<User2NodeNotifyDto>("WHERE nodeId = @nodeId", new { nodeId = entity.Id });

        public int DeleteNotifications(IScope scope, IUser user) =>
            scope.Database.Delete<User2NodeNotifyDto>("WHERE userId = @userId", new { userId = user.Id });

        public int DeleteNotifications(IScope scope, IUser user, IEntity entity) =>

            // Delete all settings on the node for this user
            scope.Database.Delete<User2NodeNotifyDto>("WHERE userId = @userId AND nodeId = @nodeId", new { userId = user.Id, nodeId = entity.Id });

        public Notification CreateNotification(IScope scope, IUser user, IEntity entity, string action)
        {
            Sql<ISqlContext> sql = scope.SqlContext.Sql()
                .Select("DISTINCT nodeObjectType")
                .From<NodeDto>()
                .Where<NodeDto>(nodeDto => nodeDto.NodeId == entity.Id);
            Guid nodeType = scope.Database.ExecuteScalar<Guid>(sql);

            var dto = new User2NodeNotifyDto
                {
                    Action = action,
                    NodeId = entity.Id,
                    UserId = user.Id
                };
            scope.Database.Insert(dto);
            return new Notification(dto.NodeId, dto.UserId, dto.Action, nodeType);
        }
    }
}
