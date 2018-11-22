using System;
using Umbraco.Core.Events;
using Umbraco.Core.Sync;
using umbraco.interfaces;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A base class for cache refreshers to inherit from that ensures the correct events are raised
    /// when cache refreshing occurs.
    /// </summary>
    /// <typeparam name="TInstanceType">The real cache refresher type, this is used for raising strongly typed events</typeparam>
    public abstract class CacheRefresherBase<TInstanceType> : ICacheRefresher
        where TInstanceType : ICacheRefresher
    {
        /// <summary>
        /// An event that is raised when cache is updated on an individual server
        /// </summary>
        /// <remarks>
        /// This event will fire on each server configured for an Umbraco project whenever a cache refresher
        /// is updated.
        /// </remarks>
        public static event TypedEventHandler<TInstanceType, CacheRefresherEventArgs> CacheUpdated;

        /// <summary>
        /// Raises the event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected static void OnCacheUpdated(TInstanceType sender, CacheRefresherEventArgs args)
        {
            if (CacheUpdated != null)
            {
                CacheUpdated(sender, args);
            }
        }

        /// <summary>
        /// Returns the real instance of the object ('this') for use  in strongly typed events
        /// </summary>
        protected abstract TInstanceType Instance { get; }

        public abstract Guid UniqueIdentifier { get; }

        public abstract string Name { get; }

        public virtual void RefreshAll()
        {
            OnCacheUpdated(Instance, new CacheRefresherEventArgs(null, MessageType.RefreshAll));
        }

        public virtual void Refresh(int id)
        {
            OnCacheUpdated(Instance, new CacheRefresherEventArgs(id, MessageType.RefreshById));
        }

        public virtual void Remove(int id)
        {
            OnCacheUpdated(Instance, new CacheRefresherEventArgs(id, MessageType.RemoveById));
        }

        public virtual void Refresh(Guid id)
        {
            OnCacheUpdated(Instance, new CacheRefresherEventArgs(id, MessageType.RefreshById));
        }

        /// <summary>
        /// Clears the cache for all repository entities of this type
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        internal void ClearAllIsolatedCacheByEntityType<TEntity>()
            where TEntity : class, IAggregateRoot
        {
            ApplicationContext.Current.ApplicationCache.IsolatedRuntimeCache.ClearCache<TEntity>();
        }
    }
}