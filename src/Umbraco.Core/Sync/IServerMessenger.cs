using System;
using System.Collections.Generic;
using umbraco.interfaces;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// Defines a server messenger for server sync and distrubuted cache
    /// </summary>
    public interface IServerMessenger
    {

        /// <summary>
        /// Performs a refresh and sends along the JSON payload to each server
        /// </summary>
        /// <param name="servers"></param>
        /// <param name="refresher"></param>
        /// <param name="jsonPayload">
        /// A pre-formatted custom json payload to be sent to the servers, the cache refresher will deserialize and use to refresh cache
        /// </param>
        void PerformRefresh(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, string jsonPayload);

        /// <summary>
        /// Performs a sync against all instance objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="servers">The servers to sync against</param>
        /// <param name="refresher"></param>
        /// <param name="getNumericId">A delegate to return the Id for each instance to be used to sync to other servers</param>
        /// <param name="instances"></param>
        void PerformRefresh<T>(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances);
        
        /// <summary>
        /// Performs a sync against all instance objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="servers">The servers to sync against</param>
        /// <param name="refresher"></param>
        /// <param name="getGuidId">A delegate to return the Id for each instance to be used to sync to other servers</param>
        /// <param name="instances"></param>
        void PerformRefresh<T>(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, Func<T, Guid> getGuidId, params T[] instances);

        /// <summary>
        /// Removes the cache for the specified items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="servers"></param>
        /// <param name="refresher"></param>
        /// <param name="getNumericId">A delegate to return the Id for each instance to be used to sync to other servers</param>
        /// <param name="instances"></param>
        void PerformRemove<T>(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances);

        /// <summary>
        /// Removes the cache for the specified items
        /// </summary>
        /// <param name="servers"></param>
        /// <param name="refresher"></param>
        /// <param name="numericIds"></param>
        void PerformRemove(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, params int[] numericIds);

        /// <summary>
        /// Performs a sync against all Ids
        /// </summary>
        /// <param name="servers">The servers to sync against</param>
        /// <param name="refresher"></param>
        /// <param name="numericIds"></param>
        void PerformRefresh(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, params int[] numericIds);
        
        /// <summary>
        /// Performs a sync against all Ids
        /// </summary>
        /// <param name="servers">The servers to sync against</param>
        /// <param name="refresher"></param>
        /// <param name="guidIds"></param>
        void PerformRefresh(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, params Guid[] guidIds);

        /// <summary>
        /// Performs entire cache refresh for a specified refresher
        /// </summary>
        /// <param name="servers"></param>
        /// <param name="refresher"></param>
        void PerformRefreshAll(IEnumerable<IServerAddress> servers, ICacheRefresher refresher);
    }

}