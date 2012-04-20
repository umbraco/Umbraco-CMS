using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Xml;
using umbraco.BusinessLogic;
using umbraco.interfaces;
using umbraco.IO;

namespace umbraco.presentation.cache
{
    /// <summary>
    /// Dispatcher is used to handle Umbraco's load balancing.
    /// Distributing calls to all registered load balanced servers, ensuring that content are synced and cached on all servers.
    /// 
    /// Dispatcher is exendable, so 3rd party services can easily be integrated into the workflow, using the interfaces.ICacheRefresher interface.
    /// 
    /// Dispatcher can refresh/remove content, templates and macros.
    /// Load balanced servers are registered in umbracoSettings.config.
    /// 
    /// UPDATE 2010 02 - Alex Norcliffe - Refactored Dispatcher to support parallel dispatch threads, and preventing failure of whole dispatch
    /// if one node fails. Still needs more work to get it to Enterprise level though but this is for 4.1
    /// </summary>
    public class dispatcher
    {
        private static readonly string _login = User.GetUser(UmbracoSettings.DistributedCallUser).LoginName;
        private static readonly string _password = User.GetUser(UmbracoSettings.DistributedCallUser).GetPassword();
        private static readonly string _webServicesUrl;


        static dispatcher()
        {
            _webServicesUrl = IOHelper.ResolveUrl(SystemDirectories.Webservices);
        }

        /// <summary>
        /// Sends a request to all registered load-balanced servers to refresh node with the specified Id
        /// using the specified ICacheRefresher with the guid factoryGuid.
        /// </summary>
        /// <param name="factoryGuid">The unique identifier of the ICacheRefresher used to refresh the node.</param>
        /// <param name="Id">The id of the node.</param>
        public static void Refresh(Guid factoryGuid, int Id)
        {
            InvokeDispatchMethod(DispatchType.RefreshByNumericId, factoryGuid, Id, Guid.Empty);
        }

        /// <summary>
        /// Sends a request to all registered load-balanced servers to refresh the node with the specified guid
        /// using the specified ICacheRefresher with the guid factoryGuid.
        /// </summary>
        /// <param name="factoryGuid">The unique identifier of the ICacheRefresher used to refresh the node.</param>
        /// <param name="Id">The guid of the node.</param>
        public static void Refresh(Guid factoryGuid, Guid Id)
        {
            InvokeDispatchMethod(DispatchType.RefreshByGuid, factoryGuid, 0, Id);
        }

        /// <summary>
        /// Sends a request to all registered load-balanced servers to refresh all nodes
        /// using the specified ICacheRefresher with the guid factoryGuid.
        /// </summary>
        /// <param name="factoryGuid">The unique identifier.</param>
        public static void RefreshAll(Guid factoryGuid)
        {
            InvokeDispatchMethod(DispatchType.RefreshAll, factoryGuid, 0, Guid.Empty);
        }

        /// <summary>
        /// Sends a request to all registered load-balanced servers to remove the node with the specified id
        /// using the specified ICacheRefresher with the guid factoryGuid.
        /// </summary>
        /// <param name="factoryGuid">The unique identifier.</param>
        /// <param name="Id">The id.</param>
        public static void Remove(Guid factoryGuid, int Id)
        {
            InvokeDispatchMethod(DispatchType.RemoveById, factoryGuid, Id, Guid.Empty);
        }

        /// <summary>
        /// Invokes the relevant dispatch method.
        /// </summary>
        /// <param name="dispatchType">Type of the dispatch.</param>
        /// <param name="factoryGuid">The factory GUID.</param>
        /// <param name="numericId">The numeric id.</param>
        /// <param name="guidId">The GUID id.</param>
        private static void InvokeDispatchMethod(DispatchType dispatchType, Guid factoryGuid, int numericId, Guid guidId)
        {
            string name = GetFactoryObjectName(factoryGuid);

            try
            {
                using (var cacheRefresher = new CacheRefresher())
                {
                    var asyncResultsList = new List<IAsyncResult>();

                    LogStartDispatch();

                    // Go through each configured node submitting a request asynchronously
                    foreach (XmlNode n in GetDistributedNodes())
                    {
                        SetWebServiceUrlFromNode(cacheRefresher, n);

                        // Add the returned WaitHandle to the list for later checking
                        switch (dispatchType)
                        {
                            case DispatchType.RefreshAll:
                                asyncResultsList.Add(cacheRefresher.BeginRefreshAll(factoryGuid, _login, _password, null,
                                                                                    null));
                                break;
                            case DispatchType.RefreshByGuid:
                                asyncResultsList.Add(cacheRefresher.BeginRefreshByGuid(factoryGuid, guidId, _login,
                                                                                       _password, null, null));
                                break;
                            case DispatchType.RefreshByNumericId:
                                asyncResultsList.Add(cacheRefresher.BeginRefreshById(factoryGuid, numericId, _login,
                                                                                     _password, null, null));
                                break;
                            case DispatchType.RemoveById:
                                asyncResultsList.Add(cacheRefresher.BeginRemoveById(factoryGuid, numericId, _login,
                                                                                    _password, null, null));
                                break;
                        }
                    }


                    List<WaitHandle> waitHandlesList;
                    IAsyncResult[] asyncResults = GetAsyncResults(asyncResultsList, out waitHandlesList);

                    int errorCount = 0;

                    // Once for each WaitHandle that we have, wait for a response and log it
                    // We're previously submitted all these requests effectively in parallel and will now retrieve responses on a FIFO basis
                    for (int waitCalls = 0; waitCalls < asyncResults.Length; waitCalls++)
                    {
                        int handleIndex = WaitHandle.WaitAny(waitHandlesList.ToArray(), TimeSpan.FromSeconds(15));

                        try
                        {
                            // Find out if the call succeeded
                            switch (dispatchType)
                            {
                                case DispatchType.RefreshAll:
                                    cacheRefresher.EndRefreshAll(asyncResults[waitCalls]);
                                    break;
                                case DispatchType.RefreshByGuid:
                                    cacheRefresher.EndRefreshByGuid(asyncResults[waitCalls]);
                                    break;
                                case DispatchType.RefreshByNumericId:
                                    cacheRefresher.EndRefreshById(asyncResults[waitCalls]);
                                    break;
                                case DispatchType.RemoveById:
                                    cacheRefresher.EndRemoveById(asyncResults[waitCalls]);
                                    break;
                            }
                        }
                        catch (WebException ex)
                        {
                            LogDispatchNodeError(ex);

                            errorCount++;
                        }
                        catch (Exception ex)
                        {
                            LogDispatchNodeError(ex);

                            errorCount++;
                        }
                    }

                    LogDispatchBatchResult(errorCount);
                }
            }
            catch (Exception ee)
            {
                LogDispatchBatchError(ee);
            }
        }

        private static void LogDispatchBatchError(Exception ee)
        {
            Log.Add(
                       LogTypes.Error,
                       User.GetUser(0),
                       -1,
                       string.Format("Error refreshing distributed list: '{0}'", ee));
        }

        private static void LogDispatchBatchResult(int errorCount)
        {
            Log.Add(
                       LogTypes.System,
                       User.GetUser(0),
                       -1,
                       string.Format("Distributed server push completed with {0} nodes reporting an error",
                                     errorCount == 0 ? "no" : errorCount.ToString()));
        }

        private static void LogDispatchNodeError(Exception ex)
        {
            Log.Add(
                       LogTypes.Error,
                       User.GetUser(0),
                       -1,
                       string.Format("Error refreshing a node in the distributed list: '{0}'", ex));
        }

        private static void LogDispatchNodeError(WebException ex)
        {
            string url = (ex.Response != null) ? ex.Response.ResponseUri.ToString() : "invalid url (responseUri null)";

            Log.Add(
                       LogTypes.Error,
                       User.GetUser(0),
                       -1,
                       string.Format("Error refreshing a node in the distributed list: '{0}', URI attempted: {1}", ex,
                                     url));
        }

        /// <summary>
        /// Sets the web service URL for a CacheRefresher from an XmlNode.
        /// </summary>
        /// <param name="cr">The CacheRefresher.</param>
        /// <param name="n">The XmlNode.</param>
        private static void SetWebServiceUrlFromNode(CacheRefresher cr, XmlNode n)
        {
            string protocol = GlobalSettings.UseSSL ? "https" : "http";
            if (n.Attributes.GetNamedItem("forceProtocol") != null && !String.IsNullOrEmpty(n.Attributes.GetNamedItem("forceProtocol").Value))
                protocol = n.Attributes.GetNamedItem("forceProtocol").Value;
            string domain = xmlHelper.GetNodeValue(n);
            if (n.Attributes.GetNamedItem("forcePortnumber") != null && !String.IsNullOrEmpty(n.Attributes.GetNamedItem("forcePortnumber").Value))
                domain += string.Format(":{0}", n.Attributes.GetNamedItem("forcePortnumber").Value);

            cr.Url = string.Format("{0}://{1}{2}/cacheRefresher.asmx", protocol, domain, _webServicesUrl);
        }

        private static string GetFactoryObjectName(Guid uniqueIdentifier)
        {
            ICacheRefresher cacheRefresher = new Factory().GetNewObject(uniqueIdentifier);

            return cacheRefresher != null ? cacheRefresher.Name : "<error determining factory type>";
        }

        private static void LogStartDispatch()
        {
            Log.Add(
                       LogTypes.System,
                       User.GetUser(0),
                       -1,
                       "Submitting calls to distributed servers");
        }

        /// <summary>
        /// Gets the node list of DistributionServers from config.
        /// </summary>
        /// <returns></returns>
        private static XmlNodeList GetDistributedNodes()
        {
            return UmbracoSettings.DistributionServers.SelectNodes("./server");
        }

        private static IAsyncResult[] GetAsyncResults(List<IAsyncResult> asyncResultsList,
                                                      out List<WaitHandle> waitHandlesList)
        {
            IAsyncResult[] asyncResults = asyncResultsList.ToArray();
            waitHandlesList = new List<WaitHandle>();
            foreach (IAsyncResult asyncResult in asyncResults)
            {
                waitHandlesList.Add(asyncResult.AsyncWaitHandle);
            }
            return asyncResults;
        }

        #region Nested type: DispatchType

        private enum DispatchType
        {
            RefreshAll,
            RefreshByNumericId,
            RefreshByGuid,
            RemoveById
        }

        #endregion
    }
}