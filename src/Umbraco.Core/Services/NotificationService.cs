using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using umbraco.interfaces;

namespace Umbraco.Core.Services
{
    internal class NotificationService : INotificationService
    {
        private readonly IDatabaseUnitOfWorkProvider _uowProvider;

        public NotificationService(IDatabaseUnitOfWorkProvider provider)
        {
            _uowProvider = provider;
        }

        /// <summary>
        /// Sends the notifications for the specified user regarding the specified node and action.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="user"></param>
        /// <param name="action"></param>
        public void SendNotifications(IEntity entity, IUser user, IAction action)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the notifications for the user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IEnumerable<Notification> GetUserNotifications(IUser user)
        {
            var uow = _uowProvider.GetUnitOfWork();
            var repository = new NotificationsRepository(uow);
            return repository.GetUserNotifications(user);
        }

        /// <summary>
        /// Deletes notifications by entity
        /// </summary>
        /// <param name="entity"></param>
        public IEnumerable<Notification> GetEntityNotifications(IEntity entity)
        {
            var uow = _uowProvider.GetUnitOfWork();
            var repository = new NotificationsRepository(uow);
            return repository.GetEntityNotifications(entity);
        }

        /// <summary>
        /// Deletes notifications by entity
        /// </summary>
        /// <param name="entity"></param>
        public void DeleteNotifications(IEntity entity)
        {
            var uow = _uowProvider.GetUnitOfWork();
            var repository = new NotificationsRepository(uow);
            repository.DeleteNotifications(entity);
        }

        /// <summary>
        /// Deletes notifications by user
        /// </summary>
        /// <param name="user"></param>
        public void DeleteNotifications(IUser user)
        {
            var uow = _uowProvider.GetUnitOfWork();
            var repository = new NotificationsRepository(uow);
            repository.DeleteNotifications(user);
        }

        /// <summary>
        /// Delete notifications by user and entity
        /// </summary>
        /// <param name="user"></param>
        /// <param name="entity"></param>
        public void DeleteNotifications(IUser user, IEntity entity)
        {
            var uow = _uowProvider.GetUnitOfWork();
            var repository = new NotificationsRepository(uow);
            repository.DeleteNotifications(user, entity);
        }

        /// <summary>
        /// Creates a new notification
        /// </summary>
        /// <param name="user"></param>
        /// <param name="entity"></param>
        /// <param name="action">The action letter - note: this is a string for future compatibility</param>
        /// <returns></returns>
        public Notification CreateNotification(IUser user, IEntity entity, string action)
        {
            var uow = _uowProvider.GetUnitOfWork();
            var repository = new NotificationsRepository(uow);
            return repository.CreateNotification(user, entity, action);
        }
    }
}