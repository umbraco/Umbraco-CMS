using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using umbraco.interfaces;

namespace Umbraco.Core.Services
{
    internal interface INotificationService : IService
    {
        /// <summary>
        /// Sends the notifications for the specified user regarding the specified node and action.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="user"></param>
        /// <param name="action"></param>
        void SendNotifications(IEntity entity, IUser user, IAction action);

        /// <summary>
        /// Gets the notifications for the user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        IEnumerable<Notification> GetUserNotifications(IUser user);

        /// <summary>
        /// Returns the notifications for an entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        IEnumerable<Notification> GetEntityNotifications(IEntity entity);

        /// <summary>
        /// Deletes notifications by entity
        /// </summary>
        /// <param name="entity"></param>
        void DeleteNotifications(IEntity entity);

        /// <summary>
        /// Deletes notifications by user
        /// </summary>
        /// <param name="user"></param>
        void DeleteNotifications(IUser user);

        /// <summary>
        /// Delete notifications by user and entity
        /// </summary>
        /// <param name="user"></param>
        /// <param name="entity"></param>
        void DeleteNotifications(IUser user, IEntity entity);

        /// <summary>
        /// Creates a new notification
        /// </summary>
        /// <param name="user"></param>
        /// <param name="entity"></param>
        /// <param name="action">The action letter - note: this is a string for future compatibility</param>
        /// <returns></returns>
        Notification CreateNotification(IUser user, IEntity entity, string action);
    }
}
