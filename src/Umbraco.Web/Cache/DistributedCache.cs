using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Web.Services.Protocols;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;
using umbraco.BusinessLogic;
using umbraco.interfaces;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// DistributedCache is used to invalidate cache throughout the application which also takes in to account load balancing environments automatically
    /// </summary>
    /// <remarks>
    /// Distributing calls to all registered load balanced servers, ensuring that content are synced and cached on all servers.
    /// Dispatcher is exendable, so 3rd party services can easily be integrated into the workflow, using the interfaces.ICacheRefresher interface.
    /// 
    /// Dispatcher can refresh/remove content, templates and macros.
    /// </remarks>
    public class DistributedCache
    {

        #region Public constants/Ids

        public const string TemplateRefresherId = "DD12B6A0-14B9-46e8-8800-C154F74047C8";
        public const string PageCacheRefresherId = "27AB3022-3DFA-47b6-9119-5945BC88FD66";
        public const string MemberCacheRefresherId = "E285DF34-ACDC-4226-AE32-C0CB5CF388DA";
        public const string MediaCacheRefresherId = "B29286DD-2D40-4DDB-B325-681226589FEC";
        public const string MacroCacheRefresherId = "7B1E683C-5F34-43dd-803D-9699EA1E98CA";
        public const string UserCacheRefresherId = "E057AF6D-2EE6-41F4-8045-3694010F0AA6";
        
        #endregion

        private static readonly DistributedCache InstanceObject = new DistributedCache();

        /// <summary>
        /// Constructor
        /// </summary>
        private DistributedCache()
        {                 
        }
        
        /// <summary>
        /// Singleton
        /// </summary>
        /// <returns></returns>
        public static DistributedCache Instance
        {
            get
            {
                return InstanceObject;    
            }
        }

        /// <summary>
        /// Sends a request to all registered load-balanced servers to refresh node with the specified Id
        /// using the specified ICacheRefresher with the guid factoryGuid.
        /// </summary>
        /// <param name="factoryGuid">The unique identifier of the ICacheRefresher used to refresh the node.</param>
        /// <param name="id">The id of the node.</param>
        public void Refresh(Guid factoryGuid, int id)
        {
            ServerMessengerResolver.Current.Messenger.PerformRefresh(
                ServerRegistrarResolver.Current.Registrar.Registrations, 
                GetRefresherById(factoryGuid), 
                id);
        }

        /// <summary>
        /// Sends a request to all registered load-balanced servers to refresh the node with the specified guid
        /// using the specified ICacheRefresher with the guid factoryGuid.
        /// </summary>
        /// <param name="factoryGuid">The unique identifier of the ICacheRefresher used to refresh the node.</param>
        /// <param name="id">The guid of the node.</param>
        public void Refresh(Guid factoryGuid, Guid id)
        {
            ServerMessengerResolver.Current.Messenger.PerformRefresh(
                ServerRegistrarResolver.Current.Registrar.Registrations,
                GetRefresherById(factoryGuid),
                id);
        }

        /// <summary>
        /// Sends a request to all registered load-balanced servers to refresh all nodes
        /// using the specified ICacheRefresher with the guid factoryGuid.
        /// </summary>
        /// <param name="factoryGuid">The unique identifier.</param>
        public void RefreshAll(Guid factoryGuid)
        {
            ServerMessengerResolver.Current.Messenger.PerformRefreshAll(
                ServerRegistrarResolver.Current.Registrar.Registrations,
                GetRefresherById(factoryGuid));
        }

        /// <summary>
        /// Sends a request to all registered load-balanced servers to remove the node with the specified id
        /// using the specified ICacheRefresher with the guid factoryGuid.
        /// </summary>
        /// <param name="factoryGuid">The unique identifier.</param>
        /// <param name="id">The id.</param>
        public void Remove(Guid factoryGuid, int id)
        {
            ServerMessengerResolver.Current.Messenger.PerformRemove(
                ServerRegistrarResolver.Current.Registrar.Registrations,
                GetRefresherById(factoryGuid),
                id);
        }

        private static ICacheRefresher GetRefresherById(Guid uniqueIdentifier)
        {
            return CacheRefreshersResolver.Current.GetById(uniqueIdentifier);
        }

    }
}