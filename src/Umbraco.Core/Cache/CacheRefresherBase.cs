using System;
using Umbraco.Core.Events;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Sync;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A base class for cache refreshers that handles events.
    /// </summary>
    /// <typeparam name="TInstanceType">The actual cache refresher type.</typeparam>
    /// <remarks>The actual cache refresher type is used for strongly typed events.</remarks>
    public abstract class CacheRefresherBase<TInstanceType> : ICacheRefresher
        where TInstanceType : class, ICacheRefresher
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheRefresherBase{TInstanceType}"/>.
        /// </summary>
        /// <param name="appCaches">A cache helper.</param>
        protected CacheRefresherBase(AppCaches appCaches)
        {
            AppCaches = appCaches;
        }

        /// <summary>
        /// Triggers when the cache is updated on the server.
        /// </summary>
        /// <remarks>
        /// Triggers on each server configured for an Umbraco project whenever a cache refresher is updated.
        /// </remarks>
        public static event TypedEventHandler<TInstanceType, CacheRefresherEventArgs> CacheUpdated;

        #region Define

        /// <summary>
        /// Gets the typed 'this' for events.
        /// </summary>
        protected abstract TInstanceType This { get; }

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
            OnCacheUpdated(This, new CacheRefresherEventArgs(null, MessageType.RefreshAll));
        }

        /// <summary>
        /// Refreshes an entity.
        /// </summary>
        /// <param name="id">The entity's identifier.</param>
        public virtual void Refresh(int id)
        {
            OnCacheUpdated(This, new CacheRefresherEventArgs(id, MessageType.RefreshById));
        }

        /// <summary>
        /// Refreshes an entity.
        /// </summary>
        /// <param name="id">The entity's identifier.</param>
        public virtual void Refresh(Guid id)
        {
            OnCacheUpdated(This, new CacheRefresherEventArgs(id, MessageType.RefreshById));
        }

        /// <summary>
        /// Removes an entity.
        /// </summary>
        /// <param name="id">The entity's identifier.</param>
        public virtual void Remove(int id)
        {
            OnCacheUpdated(This, new CacheRefresherEventArgs(id, MessageType.RemoveById));
        }

        #endregion

        #region Protected

        /// <summary>
        /// Gets the cache helper.
        /// </summary>
        protected AppCaches AppCaches { get; }

        /// <summary>
        /// Clears the cache for all repository entities of a specified type.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        protected void ClearAllIsolatedCacheByEntityType<TEntity>()
            where TEntity : class, IEntity
        {
            AppCaches.IsolatedRuntimeCache.ClearCache<TEntity>();
        }

        /// <summary>
        /// Raises the CacheUpdated event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        protected static void OnCacheUpdated(TInstanceType sender, CacheRefresherEventArgs args)
        {
            CacheUpdated?.Invoke(sender, args);
        }

        #endregion
    }
}
