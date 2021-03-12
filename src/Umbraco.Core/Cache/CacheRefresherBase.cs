using System;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Cache
{
    /// <summary>
    /// A base class for cache refreshers that handles events.
    /// </summary>
    /// <typeparam name="TInstanceType">The actual cache refresher type.</typeparam>
    /// <remarks>The actual cache refresher type is used for strongly typed events.</remarks>
    public abstract class CacheRefresherBase< TNotification> : ICacheRefresher
        where TNotification : CacheRefresherNotificationBase, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheRefresherBase{TInstanceType}"/>.
        /// </summary>
        /// <param name="appCaches">A cache helper.</param>
        protected CacheRefresherBase(AppCaches appCaches, IEventAggregator eventAggregator)
        {
            AppCaches = appCaches;
            EventAggregator = eventAggregator;
        }

        #region Define

        /// <summary>
        /// Gets the unique identifier of the refresher.
        /// </summary>
        public abstract Guid RefresherUniqueId { get; }

        /// <summary>
        /// Gets the name of the refresher.
        /// </summary>
        public abstract string Name { get; }

        #endregion

        #region Refresher

        /// <summary>
        /// Refreshes all entities.
        /// </summary>
        public virtual void RefreshAll()
        {
            OnCacheUpdated(new TNotification().Init(null, MessageType.RefreshAll));
        }

        /// <summary>
        /// Refreshes an entity.
        /// </summary>
        /// <param name="id">The entity's identifier.</param>
        public virtual void Refresh(int id)
        {
            OnCacheUpdated(new TNotification().Init(id, MessageType.RefreshById));
        }

        /// <summary>
        /// Refreshes an entity.
        /// </summary>
        /// <param name="id">The entity's identifier.</param>
        public virtual void Refresh(Guid id)
        {
            OnCacheUpdated(new TNotification().Init(id, MessageType.RefreshById));
        }

        /// <summary>
        /// Removes an entity.
        /// </summary>
        /// <param name="id">The entity's identifier.</param>
        public virtual void Remove(int id)
        {
            OnCacheUpdated(new TNotification().Init(id, MessageType.RemoveById));
        }

        #endregion

        #region Protected

        /// <summary>
        /// Gets the cache helper.
        /// </summary>
        protected AppCaches AppCaches { get; }

        protected IEventAggregator EventAggregator { get; }

        /// <summary>
        /// Clears the cache for all repository entities of a specified type.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        protected void ClearAllIsolatedCacheByEntityType<TEntity>()
            where TEntity : class, IEntity
        {
            AppCaches.IsolatedCaches.ClearCache<TEntity>();
        }

        /// <summary>
        /// Raises the CacheUpdated event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        protected void OnCacheUpdated(CacheRefresherNotificationBase notification)
        {
            EventAggregator.Publish(notification);
        }

        #endregion
    }
}
