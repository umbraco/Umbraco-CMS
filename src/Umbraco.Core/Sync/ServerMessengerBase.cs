using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using umbraco.interfaces;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// Provides a base class for all <see cref="IServerMessenger"/> implementations.
    /// </summary>
    public abstract class ServerMessengerBase : IServerMessenger
    {
        protected bool DistributedEnabled { get; set; }

        protected ServerMessengerBase(bool distributedEnabled)
        {
            DistributedEnabled = distributedEnabled;
        }

        /// <summary>
        /// Determines whether to make distributed calls when messaging a cache refresher.
        /// </summary>
        /// <param name="servers">The registered servers.</param>
        /// <param name="refresher">The cache refresher.</param>
        /// <param name="messageType">The message type.</param>
        /// <returns>true if distributed calls are required; otherwise, false, all we have is the local server.</returns>
        protected virtual bool RequiresDistributed(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, MessageType messageType)
        {
            return DistributedEnabled && servers.Any();
        }

        // ensures that all items in the enumerable are of the same type, either int or Guid.
        protected static bool GetArrayType(IEnumerable<object> ids, out Type arrayType)
        {
            arrayType = null;
            if (ids == null) return true;

            foreach (var id in ids)
            {
                // only int and Guid are supported
                if ((id is int) == false && ((id is Guid) == false))
                    return false;
                // initialize with first item
                if (arrayType == null)
                    arrayType = id.GetType();
                // check remaining items
                if (arrayType != id.GetType())
                    return false;
            }

            return true;
        }

        #region IServerMessenger

        public void PerformRefresh(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, string jsonPayload)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");
            if (jsonPayload == null) throw new ArgumentNullException("jsonPayload");

            Deliver(servers, refresher, MessageType.RefreshByJson, json: jsonPayload);
        }

        public void PerformRefresh<T>(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");
            if (getNumericId == null) throw new ArgumentNullException("getNumericId");
            if (instances == null || instances.Length == 0) return;

            Func<T, object> getId = x => getNumericId(x);
            Deliver(servers, refresher, MessageType.RefreshByInstance, getId, instances);
        }

        public void PerformRefresh<T>(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, Func<T, Guid> getGuidId, params T[] instances)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");
            if (getGuidId == null) throw new ArgumentNullException("getGuidId");
            if (instances == null || instances.Length == 0) return;

            Func<T, object> getId = x => getGuidId(x);
            Deliver(servers, refresher, MessageType.RefreshByInstance, getId, instances);
        }

        public void PerformRemove<T>(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");
            if (getNumericId == null) throw new ArgumentNullException("getNumericId");
            if (instances == null || instances.Length == 0) return;

            Func<T, object> getId = x => getNumericId(x);
            Deliver(servers, refresher, MessageType.RemoveByInstance, getId, instances);
        }

        public void PerformRemove(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, params int[] numericIds)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");
            if (numericIds == null || numericIds.Length == 0) return;

            Deliver(servers, refresher, MessageType.RemoveById, numericIds.Cast<object>());
        }

        public void PerformRefresh(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, params int[] numericIds)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");
            if (numericIds == null || numericIds.Length == 0) return;

            Deliver(servers, refresher, MessageType.RefreshById, numericIds.Cast<object>());
        }

        public void PerformRefresh(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, params Guid[] guidIds)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");
            if (guidIds == null || guidIds.Length == 0) return;

            Deliver(servers, refresher, MessageType.RefreshById, guidIds.Cast<object>());
        }

        public void PerformRefreshAll(IEnumerable<IServerAddress> servers, ICacheRefresher refresher)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");

            Deliver(servers, refresher, MessageType.RefreshAll);
        }

        //public void PerformNotify(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, object payload)
        //{
        //    if (servers == null) throw new ArgumentNullException("servers");
        //    if (refresher == null) throw new ArgumentNullException("refresher");

        //    Deliver(servers, refresher, payload);
        //}

        #endregion

        #region Deliver

        /// <summary>
        /// Executes the non strongly typed <see cref="ICacheRefresher"/> on the local/current server
        /// </summary>
        /// <param name="refresher"></param>
        /// <param name="messageType"></param>
        /// <param name="ids"></param>
        /// <param name="json"></param>
        /// <remarks>
        /// Since this is only for non strongly typed <see cref="ICacheRefresher"/> it will throw for message types that by instance
        /// </remarks>
        protected void DeliverLocal(ICacheRefresher refresher, MessageType messageType, IEnumerable<object> ids = null, string json = null)
        {
            if (refresher == null) throw new ArgumentNullException("refresher");

            LogHelper.Debug<ServerMessengerBase>("Invoking refresher {0} on local server for message type {1}",
                refresher.GetType,
                () => messageType);

            switch (messageType)
            {
                case MessageType.RefreshAll:
                    refresher.RefreshAll();
                    break;

                case MessageType.RefreshById:
                    if (ids != null)
                        foreach (var id in ids)
                        {
                            if (id is int)
                                refresher.Refresh((int) id);
                            else if (id is Guid)
                                refresher.Refresh((Guid) id);
                            else
                                throw new InvalidOperationException("The id must be either an int or a Guid.");
                        }
                    break;

                case MessageType.RefreshByJson:
                    var jsonRefresher = refresher as IJsonCacheRefresher;
                    if (jsonRefresher == null)
                        throw new InvalidOperationException("The cache refresher " + refresher.GetType() + " is not of type " + typeof(IJsonCacheRefresher));
                    jsonRefresher.Refresh(json);
                    break;

                case MessageType.RemoveById:
                    if (ids != null)
                        foreach (var id in ids)
                        {
                            if (id is int)
                                refresher.Remove((int) id);
                            else
                                throw new InvalidOperationException("The id must be an int.");
                        }
                    break;

                default:
                //case MessageType.RefreshByInstance:
                //case MessageType.RemoveByInstance:
                    throw new NotSupportedException("Invalid message type " + messageType);
            }
        }
        
        /// <summary>
        /// Executes the strongly typed <see cref="ICacheRefresher{T}"/> on the local/current server
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="refresher"></param>
        /// <param name="messageType"></param>
        /// <param name="getId"></param>
        /// <param name="instances"></param>
        /// <remarks>
        /// Since this is only for strongly typed <see cref="ICacheRefresher{T}"/> it will throw for message types that are not by instance
        /// </remarks>
        protected void DeliverLocal<T>(ICacheRefresher refresher, MessageType messageType, Func<T, object> getId, IEnumerable<T> instances)
        {
            if (refresher == null) throw new ArgumentNullException("refresher");

            LogHelper.Debug<ServerMessengerBase>("Invoking refresher {0} on local server for message type {1}",
                refresher.GetType,
                () => messageType);

            var typedRefresher = refresher as ICacheRefresher<T>;

            switch (messageType)
            {
                case MessageType.RefreshAll:
                    refresher.RefreshAll();
                    break;

                case MessageType.RefreshByInstance:
                    if (typedRefresher == null)
                        throw new InvalidOperationException("The refresher must be a typed refresher.");
                    foreach (var instance in instances)
                        typedRefresher.Refresh(instance);
                    break;

                case MessageType.RemoveByInstance:
                    if (typedRefresher == null)
                        throw new InvalidOperationException("The cache refresher " + refresher.GetType() + " is not a typed refresher.");
                    foreach (var instance in instances)
                        typedRefresher.Remove(instance);
                    break;

                default:
                //case MessageType.RefreshById:
                //case MessageType.RemoveById:
                //case MessageType.RefreshByJson:
                    throw new NotSupportedException("Invalid message type " + messageType);
            }
        }

        //protected void DeliverLocal(ICacheRefresher refresher, object payload)
        //{
        //    if (refresher == null) throw new ArgumentNullException("refresher");

        //    LogHelper.Debug<ServerMessengerBase>("Invoking refresher {0} on local server for message type Notify",
        //        () => refresher.GetType());

        //    refresher.Notify(payload);
        //}

        protected abstract void DeliverRemote(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, MessageType messageType, IEnumerable<object> ids = null, string json = null);

        //protected abstract void DeliverRemote(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, object payload);

        protected virtual void Deliver(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, MessageType messageType, IEnumerable<object> ids = null, string json = null)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");

            var serversA = servers.ToArray();
            var idsA = ids == null ? null : ids.ToArray();

            // deliver local
            DeliverLocal(refresher, messageType, idsA, json);

            // distribute?
            if (RequiresDistributed(serversA, refresher, messageType) == false)
                return;

            // deliver remote
            DeliverRemote(serversA, refresher, messageType, idsA, json);
        }

        protected virtual void Deliver<T>(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, MessageType messageType, Func<T, object> getId, IEnumerable<T> instances)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");

            var serversA = servers.ToArray();
            var instancesA = instances.ToArray();

            // deliver local
            DeliverLocal(refresher, messageType, getId, instancesA);

            // distribute?
            if (RequiresDistributed(serversA, refresher, messageType) == false)
                return;

            // deliver remote

            // map ByInstance to ById as there's no remote instances
            if (messageType == MessageType.RefreshByInstance) messageType = MessageType.RefreshById;
            if (messageType == MessageType.RemoveByInstance) messageType = MessageType.RemoveById;

            // convert instances to identifiers
            var idsA = instancesA.Select(getId).ToArray();

            DeliverRemote(serversA, refresher, messageType, idsA);
        }

        //protected virtual void Deliver(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, object payload)
        //{
        //    if (servers == null) throw new ArgumentNullException("servers");
        //    if (refresher == null) throw new ArgumentNullException("refresher");

        //    var serversA = servers.ToArray();

        //    // deliver local
        //    DeliverLocal(refresher, payload);

        //    // distribute?
        //    if (RequiresDistributed(serversA, refresher, messageType) == false)
        //        return;

        //    // deliver remote
        //    DeliverRemote(serversA, refresher, payload);
        //}

        #endregion
    }
}
