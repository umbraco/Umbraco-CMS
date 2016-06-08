using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Sync;
using Umbraco.Core.Cache;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Represents the entry point into Umbraco's distributed cache infrastructure.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The distributed cache infrastructure ensures that distributed caches are
    /// invalidated properly in load balancing environments.
    /// </para>
    /// <para>
    /// Distribute caches include static (in-memory) cache, runtime cache, front-end content cache, Examine/Lucene indexes
    /// </para>
    /// </remarks>
    public sealed class DistributedCache
    {
        #region Constructor & Singleton

        // note - should inject into the application instead of using a singleton
        private static readonly DistributedCache InstanceObject = new DistributedCache();

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedCache"/> class.
        /// </summary>
        private DistributedCache()
        { }

        /// <summary>
        /// Gets the static unique instance of the <see cref="DistributedCache"/> class.
        /// </summary>
        /// <returns>The static unique instance of the <see cref="DistributedCache"/> class.</returns>
        /// <remarks>Exists so that extension methods can be added to the distributed cache.</remarks>
        public static DistributedCache Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        #endregion

        #region Core notification methods

        /// <summary>
        /// Notifies the distributed cache of specifieds item invalidation, for a specified <see cref="ICacheRefresher"/>.
        /// </summary>
        /// <typeparam name="T">The type of the invalidated items.</typeparam>
        /// <param name="refresherGuid">The unique identifier of the ICacheRefresher.</param>
        /// <param name="getNumericId">A function returning the unique identifier of items.</param>
        /// <param name="instances">The invalidated items.</param>
        /// <remarks>
        /// This method is much better for performance because it does not need to re-lookup object instances.
        /// </remarks>
        public void Refresh<T>(Guid refresherGuid, Func<T, int> getNumericId, params T[] instances)
        {
            if (refresherGuid == Guid.Empty || instances.Length == 0 || getNumericId == null) return;

            ServerMessengerResolver.Current.Messenger.PerformRefresh(
                ServerRegistrarResolver.Current.Registrar.Registrations,
                GetRefresherById(refresherGuid),
                getNumericId,
                instances);
        }

        /// <summary>
        /// Notifies the distributed cache of a specified item invalidation, for a specified <see cref="ICacheRefresher"/>.
        /// </summary>
        /// <param name="refresherGuid">The unique identifier of the ICacheRefresher.</param>
        /// <param name="id">The unique identifier of the invalidated item.</param>
        public void Refresh(Guid refresherGuid, int id)
        {
            if (refresherGuid == Guid.Empty || id == default(int)) return;

            ServerMessengerResolver.Current.Messenger.PerformRefresh(
                ServerRegistrarResolver.Current.Registrar.Registrations,
                GetRefresherById(refresherGuid),
                id);
        }

        /// <summary>
        /// Notifies the distributed cache of a specified item invalidation, for a specified <see cref="ICacheRefresher"/>.
        /// </summary>
        /// <param name="refresherGuid">The unique identifier of the ICacheRefresher.</param>
        /// <param name="id">The unique identifier of the invalidated item.</param>
        public void Refresh(Guid refresherGuid, Guid id)
        {
            if (refresherGuid == Guid.Empty || id == Guid.Empty) return;

            ServerMessengerResolver.Current.Messenger.PerformRefresh(
                ServerRegistrarResolver.Current.Registrar.Registrations,
                GetRefresherById(refresherGuid),
                id);
        }

        // payload should be an object, or array of objects, NOT a
        // Linq enumerable of some sort (IEnumerable, query...)
        public void RefreshByPayload<TPayload>(Guid refresherGuid, TPayload[] payload)
        {
            if (refresherGuid == Guid.Empty || payload == null) return;

            ServerMessengerResolver.Current.Messenger.PerformRefresh(
                ServerRegistrarResolver.Current.Registrar.Registrations,
                GetRefresherById(refresherGuid),
                payload);
        }

        // so deal with IEnumerable
        public void RefreshByPayload<TPayload>(Guid refresherGuid, IEnumerable<TPayload> payloads)
            where TPayload : class
        {
            if (refresherGuid == Guid.Empty || payloads == null) return;

            ServerMessengerResolver.Current.Messenger.PerformRefresh(
                ServerRegistrarResolver.Current.Registrar.Registrations,
                GetRefresherById(refresherGuid),
                payloads.ToArray());
        }
        /// <summary>
        /// Notifies the distributed cache, for a specified <see cref="ICacheRefresher"/>.
        /// </summary>
        /// <param name="refresherGuid">The unique identifier of the ICacheRefresher.</param>
        /// <param name="jsonPayload">The notification content.</param>
        public void RefreshByJson(Guid refresherGuid, string jsonPayload)
        {
            if (refresherGuid == Guid.Empty || jsonPayload.IsNullOrWhiteSpace()) return;

            ServerMessengerResolver.Current.Messenger.PerformRefresh(
                ServerRegistrarResolver.Current.Registrar.Registrations,
                GetRefresherById(refresherGuid),
                jsonPayload);
        }

        ///// <summary>
        ///// Notifies the distributed cache, for a specified <see cref="ICacheRefresher"/>.
        ///// </summary>
        ///// <param name="refresherId">The unique identifier of the ICacheRefresher.</param>
        ///// <param name="payload">The notification content.</param>
        //internal void Notify(Guid refresherId, object payload)
        //{
        //    if (refresherId == Guid.Empty || payload == null) return;

        //    ServerMessengerResolver.Current.Messenger.Notify(
        //        ServerRegistrarResolver.Current.Registrar.Registrations,
        //        GetRefresherById(refresherId),
        //        json);
        //}

        /// <summary>
        /// Notifies the distributed cache of a global invalidation for a specified <see cref="ICacheRefresher"/>.
        /// </summary>
        /// <param name="refresherGuid">The unique identifier of the ICacheRefresher.</param>
        public void RefreshAll(Guid refresherGuid)
        {
            if (refresherGuid == Guid.Empty) return;

            ServerMessengerResolver.Current.Messenger.PerformRefreshAll(
                ServerRegistrarResolver.Current.Registrar.Registrations,
                GetRefresherById(refresherGuid));
        }

        /// <summary>
        /// Notifies the distributed cache of a specified item removal, for a specified <see cref="ICacheRefresher"/>.
        /// </summary>
        /// <param name="refresherGuid">The unique identifier of the ICacheRefresher.</param>
        /// <param name="id">The unique identifier of the removed item.</param>
        public void Remove(Guid refresherGuid, int id)
        {
            if (refresherGuid == Guid.Empty || id == default(int)) return;

            ServerMessengerResolver.Current.Messenger.PerformRemove(
                ServerRegistrarResolver.Current.Registrar.Registrations,
                GetRefresherById(refresherGuid),
                id);
        }

        /// <summary>
        /// Notifies the distributed cache of specifieds item removal, for a specified <see cref="ICacheRefresher"/>.
        /// </summary>
        /// <typeparam name="T">The type of the removed items.</typeparam>
        /// <param name="refresherGuid">The unique identifier of the ICacheRefresher.</param>
        /// <param name="getNumericId">A function returning the unique identifier of items.</param>
        /// <param name="instances">The removed items.</param>
        /// <remarks>
        /// This method is much better for performance because it does not need to re-lookup object instances.
        /// </remarks>
        public void Remove<T>(Guid refresherGuid, Func<T, int> getNumericId, params T[] instances)
        {
            ServerMessengerResolver.Current.Messenger.PerformRemove(
                ServerRegistrarResolver.Current.Registrar.Registrations,
                GetRefresherById(refresherGuid),
                getNumericId,
                instances);
        }

        #endregion

        // helper method to get an ICacheRefresher by its unique identifier
        private static ICacheRefresher GetRefresherById(Guid refresherGuid)
        {
            return CacheRefreshersResolver.Current.GetById(refresherGuid);
        }
    }
}
