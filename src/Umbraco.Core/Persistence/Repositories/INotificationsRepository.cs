using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface INotificationsRepository : IRepository
{
    Notification CreateNotification(IUser user, IEntity entity, string action);

    int DeleteNotifications(IUser user);

    int DeleteNotifications(IEntity entity);

    int DeleteNotifications(IUser user, IEntity entity);

    IEnumerable<Notification>? GetEntityNotifications(IEntity entity);

    IEnumerable<Notification>? GetUserNotifications(IUser user);

    IEnumerable<Notification>? GetUsersNotifications(IEnumerable<int> userIds, string? action, IEnumerable<int> nodeIds, Guid objectType);

    IEnumerable<Notification> SetNotifications(IUser user, IEntity entity, string[] actions);
}
