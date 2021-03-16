using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Persistence.Repositories
{
    public interface INotificationsRepository : IRepository
    {
        Notification CreateNotification(IScope scope, IUser user, IEntity entity, string action);

        int DeleteNotifications(IScope scope, IUser user);

        int DeleteNotifications(IScope scope, IEntity entity);

        int DeleteNotifications(IScope scope, IUser user, IEntity entity);

        IEnumerable<Notification> GetEntityNotifications(IScope scope, IEntity entity);

        IEnumerable<Notification> GetUserNotifications(IScope scope, IUser user);

        IEnumerable<Notification> GetUsersNotifications(IScope scope, IEnumerable<int> userIds, string action, IEnumerable<int> nodeIds, Guid objectType);

        IEnumerable<Notification> SetNotifications(IScope scope, IUser user, IEntity entity, string[] actions);
    }
}
