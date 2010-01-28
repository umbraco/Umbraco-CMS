using System;
using System.Xml;
using umbraco.IO;

namespace umbraco.presentation.cache {
    /// <summary>
    /// Dispatcher is used to handle Umbraco's load balancing.
    /// Distributing calls to all registered load balanced servers, ensuring that content are synced and cached on all servers.
    /// 
    /// Dispatcher is exendable, so 3rd party services can easily be integrated into the workflow, using the interfaces.ICacheRefresher interface.
    /// 
    /// Dispatcher can refresh/remove content, templates and macros.
    /// Load balanced servers are registered in umbracoSettings.config.
    /// </summary>
    public class dispatcher {
        private static string _login = BusinessLogic.User.GetUser(UmbracoSettings.DistributedCallUser).LoginName;
        private static string _password = BusinessLogic.User.GetUser(UmbracoSettings.DistributedCallUser).GetPassword();


        /// <summary>
        /// Initializes a new instance of the <see cref="dispatcher"/> class.
        /// </summary>
        public dispatcher() {
        }

        /// <summary>
        /// Sends a request to all registered load-balanced servers to refresh node with the specified Id
        /// using the specified ICacheRefresher with the guid uniqueIdentifier.
        /// </summary>
        /// <param name="uniqueIdentifier">The unique identifier of the ICacheRefresher used to refresh the node.</param>
        /// <param name="Id">The id of the node.</param>
        public static void Refresh(Guid uniqueIdentifier, int Id) {
            try {
                using (CacheRefresher cr = new CacheRefresher())
                {
                    foreach (XmlNode n in UmbracoSettings.DistributionServers.SelectNodes("./server"))
                    {
                        cr.Url = "http://" + xmlHelper.GetNodeValue(n) + SystemDirectories.Webservices + "/cacheRefresher.asmx";
                        cr.RefreshById(uniqueIdentifier, Id, _login, _password);
                    }
                }
            } catch (Exception ee) {
                BusinessLogic.Log.Add(
                    BusinessLogic.LogTypes.Error,
                    BusinessLogic.User.GetUser(0),
                    -1,
                    "Error refreshing '" + new Factory().GetNewObject(uniqueIdentifier).Name + "' with id '" + Id.ToString() + "', error: '" + ee.ToString() + "'");
            }
        }

        /// <summary>
        /// Sends a request to all registered load-balanced servers to refresh the node with the specified guid
        /// using the specified ICacheRefresher with the guid uniqueIdentifier.
        /// </summary>
        /// <param name="uniqueIdentifier">The unique identifier of the ICacheRefresher used to refresh the node.</param>
        /// <param name="Id">The guid of the node.</param>
        public static void Refresh(Guid uniqueIdentifier, Guid Id) {
            try {
                using (CacheRefresher cr = new CacheRefresher())
                {
                    foreach (XmlNode n in UmbracoSettings.DistributionServers.SelectNodes("./server"))
                    {
                        cr.Url = "http://" + xmlHelper.GetNodeValue(n) + SystemDirectories.Webservices + "/cacheRefresher.asmx";
                        cr.RefreshByGuid(uniqueIdentifier, Id, _login, _password);
                    }
                }
            } catch (Exception ee) {
                BusinessLogic.Log.Add(
                    BusinessLogic.LogTypes.Error,
                    BusinessLogic.User.GetUser(0),
                    -1,
                    "Error refreshing '" + new Factory().GetNewObject(uniqueIdentifier).Name + "' with id '" + Id.ToString() + "', error: '" + ee.ToString() + "'");
            }
        }

        /// <summary>
        /// Sends a request to all registered load-balanced servers to refresh all nodes
        /// using the specified ICacheRefresher with the guid uniqueIdentifier.
        /// </summary>
        /// <param name="uniqueIdentifier">The unique identifier.</param>
        public static void RefreshAll(Guid uniqueIdentifier) {
            try {
                using (CacheRefresher cr = new CacheRefresher())
                {
                    foreach (XmlNode n in UmbracoSettings.DistributionServers.SelectNodes("./server"))
                    {
                        cr.Url = "http://" + xmlHelper.GetNodeValue(n) + SystemDirectories.Webservices + "/cacheRefresher.asmx";
                        cr.RefreshAll(uniqueIdentifier, _login, _password);
                    }
                }
            } catch (Exception ee) {
                BusinessLogic.Log.Add(
                    BusinessLogic.LogTypes.Error,
                    BusinessLogic.User.GetUser(0),
                    -1,
                    "Error refreshing all in '" + new Factory().GetNewObject(uniqueIdentifier).Name + "', error: '" + ee.ToString() + "'");
            }

        }

        /// <summary>
        /// Sends a request to all registered load-balanced servers to remove the node with the specified id
        /// using the specified ICacheRefresher with the guid uniqueIdentifier.
        /// </summary>
        /// <param name="uniqueIdentifier">The unique identifier.</param>
        /// <param name="Id">The id.</param>
        public static void Remove(Guid uniqueIdentifier, int Id) {

            try {
                using (                    CacheRefresher cr = new CacheRefresher()) {
                    foreach (XmlNode n in UmbracoSettings.DistributionServers.SelectNodes("./server"))
                    {
                        cr.Url = "http://" + xmlHelper.GetNodeValue(n) + SystemDirectories.Webservices + "/cacheRefresher.asmx";
                        cr.RemoveById(uniqueIdentifier, Id, _login, _password);
                    }
                }
            } catch (Exception ee) {
                BusinessLogic.Log.Add(
                    BusinessLogic.LogTypes.Error,
                    BusinessLogic.User.GetUser(0),
                    -1,
                    "Error refreshing '" + new Factory().GetNewObject(uniqueIdentifier).Name + "' with id '" + Id.ToString() + "', error: '" + ee.ToString() + "'");
            }
        }

    }
}
