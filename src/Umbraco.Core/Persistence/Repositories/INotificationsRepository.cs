using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="Notification" /> entities.
/// </summary>
public interface INotificationsRepository : IRepository
{
    /// <summary>
    ///     Tries to create a notification for a user and entity.
    /// </summary>
    /// <param name="user">The user to create the notification for.</param>
    /// <param name="entity">The entity related to the notification.</param>
    /// <param name="action">The action that triggered the notification.</param>
    /// <param name="notification">When successful, contains the created notification.</param>
    /// <returns><c>true</c> if the notification was created successfully; otherwise, <c>false</c>.</returns>
    bool TryCreateNotification(IUser user, IEntity entity, string action, [NotNullWhen(true)] out Notification? notification);

    /// <summary>
    ///     Deletes all notifications for a user.
    /// </summary>
    /// <param name="user">The user whose notifications should be deleted.</param>
    /// <returns>The number of notifications deleted.</returns>
    int DeleteNotifications(IUser user);

    /// <summary>
    ///     Deletes all notifications for an entity.
    /// </summary>
    /// <param name="entity">The entity whose notifications should be deleted.</param>
    /// <returns>The number of notifications deleted.</returns>
    int DeleteNotifications(IEntity entity);

    /// <summary>
    ///     Deletes all notifications for a specific user and entity combination.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="entity">The entity.</param>
    /// <returns>The number of notifications deleted.</returns>
    int DeleteNotifications(IUser user, IEntity entity);

    /// <summary>
    ///     Gets all notifications for an entity.
    /// </summary>
    /// <param name="entity">The entity to get notifications for.</param>
    /// <returns>A collection of notifications.</returns>
    IEnumerable<Notification> GetEntityNotifications(IEntity entity);

    /// <summary>
    ///     Gets all notifications for a user.
    /// </summary>
    /// <param name="user">The user to get notifications for.</param>
    /// <returns>A collection of notifications.</returns>
    IEnumerable<Notification> GetUserNotifications(IUser user);

    /// <summary>
    ///     Gets notifications for multiple users, filtered by action, node IDs, and object type.
    /// </summary>
    /// <param name="userIds">The user identifiers.</param>
    /// <param name="action">The action to filter by, or <c>null</c> for all actions.</param>
    /// <param name="nodeIds">The node identifiers to filter by.</param>
    /// <param name="objectType">The object type to filter by.</param>
    /// <returns>A collection of notifications.</returns>
    IEnumerable<Notification> GetUsersNotifications(IEnumerable<int> userIds, string? action, IEnumerable<int> nodeIds, Guid objectType);

    /// <summary>
    ///     Sets the notifications for a user and entity, replacing any existing notifications.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="entity">The entity.</param>
    /// <param name="actions">The actions to set notifications for.</param>
    /// <returns>A collection of the created notifications.</returns>
    IEnumerable<Notification> SetNotifications(IUser user, IEntity entity, string[] actions);
}
