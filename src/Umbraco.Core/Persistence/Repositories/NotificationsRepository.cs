using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class NotificationsRepository
    {
        private readonly IDatabaseUnitOfWork _unitOfWork;

        public NotificationsRepository(IDatabaseUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Notification> GetUserNotifications(IUser user)
        {
            var sql = new Sql()
                .Select("DISTINCT umbracoNode.id, umbracoUser2NodeNotify.userId, umbracoNode.nodeObjectType, umbracoUser2NodeNotify.action")
                .From<User2NodeNotifyDto>()
                .InnerJoin<NodeDto>()
                .On<User2NodeNotifyDto, NodeDto>(dto => dto.NodeId, dto => dto.NodeId)
                .Where<User2NodeNotifyDto>(dto => dto.UserId == (int)user.Id)
                .OrderBy<NodeDto>(dto => dto.NodeId);

            var dtos = _unitOfWork.Database.Fetch<dynamic>(sql);
            //need to map the results
            return dtos.Select(d => new Notification(d.id, d.userId, d.action, d.nodeObjectType)).ToList();
        }

        public IEnumerable<Notification> GetEntityNotifications(IEntity entity)
        {
            var sql = new Sql()
                .Select("DISTINCT umbracoNode.id, umbracoUser2NodeNotify.userId, umbracoNode.nodeObjectType, umbracoUser2NodeNotify.action")
                .From<User2NodeNotifyDto>()
                .InnerJoin<NodeDto>()
                .On<User2NodeNotifyDto, NodeDto>(dto => dto.NodeId, dto => dto.NodeId)
                .Where<User2NodeNotifyDto>(dto => dto.NodeId == entity.Id)
                .OrderBy<NodeDto>(dto => dto.NodeId);

            var dtos = _unitOfWork.Database.Fetch<dynamic>(sql);
            //need to map the results
            return dtos.Select(d => new Notification(d.id, d.userId, d.action, d.nodeObjectType)).ToList();
        }

        public int DeleteNotifications(IEntity entity)
        {
            return _unitOfWork.Database.Delete<User2NodeNotifyDto>("WHERE nodeId = @nodeId", new { nodeId = entity.Id });
        }

        public int DeleteNotifications(IUser user)
        {
            return _unitOfWork.Database.Delete<User2NodeNotifyDto>("WHERE userId = @userId", new { userId = user.Id });
        }

        public int DeleteNotifications(IUser user, IEntity entity)
        {
            // delete all settings on the node for this user
            return _unitOfWork.Database.Delete<User2NodeNotifyDto>("WHERE userId = @userId AND nodeId = @nodeId", new { userId = user.Id, nodeId = entity.Id });
        }

        public Notification CreateNotification(IUser user, IEntity entity, string action)
        {
            var sql = new Sql()
                .Select("DISTINCT nodeObjectType")
                .From<NodeDto>()
                .Where<NodeDto>(nodeDto => nodeDto.NodeId == entity.Id);
            var nodeType = _unitOfWork.Database.ExecuteScalar<Guid>(sql);

            var dto = new User2NodeNotifyDto()
                {
                    Action = action,
                    NodeId = entity.Id,
                    UserId = (int)user.Id
                };
            _unitOfWork.Database.Insert(dto);
            return new Notification(dto.NodeId, dto.UserId, dto.Action, nodeType);
        }
    }
}