using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using umbraco.interfaces;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// The default server messenger that uses web services to keep servers in sync
    /// </summary>
    internal class DefaultServerMessenger : IServerMessenger
    {
        private readonly string _login;
        private readonly string _password;
        private readonly bool _useDistributedCalls;

        /// <summary>
        /// Without a username/password all distribuion will be disabled
        /// </summary>
        internal DefaultServerMessenger()
        {
            _useDistributedCalls = false;
        }

        /// <summary>
        /// Distribution will be enabled based on the umbraco config setting.
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        internal DefaultServerMessenger(string login, string password)
        {
            _useDistributedCalls = UmbracoSettings.UseDistributedCalls;
            _login = login;
            _password = password;
        }

        public void PerformRefresh<T>(IEnumerable<IServerRegistration> servers, ICacheRefresher refresher,Func<T, int> getNumericId, params T[] instances)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");

            instances.ForEach(x => InvokeDispatchMethod(servers, refresher, MessageType.RefreshById, getNumericId(x)));
        }

        public void PerformRefresh<T>(IEnumerable<IServerRegistration> servers, ICacheRefresher refresher, Func<T, Guid> getGuidId, params T[] instances)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");

            instances.ForEach(x => InvokeDispatchMethod(servers, refresher, MessageType.RefreshById, getGuidId(x)));
        }

        public void PerformRemove<T>(IEnumerable<IServerRegistration> servers, ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");

            instances.ForEach(x => InvokeDispatchMethod(servers, refresher, MessageType.RemoveById, getNumericId(x)));
        }

        public void PerformRemove(IEnumerable<IServerRegistration> servers, ICacheRefresher refresher, params int[] numericIds)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");

            numericIds.ForEach(x => InvokeDispatchMethod(servers, refresher, MessageType.RemoveById, x));
        }

        public void PerformRefresh(IEnumerable<IServerRegistration> servers, ICacheRefresher refresher, params int[] numericIds)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");

            numericIds.ForEach(x => InvokeDispatchMethod(servers, refresher, MessageType.RefreshById, x));
        }

        public void PerformRefresh(IEnumerable<IServerRegistration> servers, ICacheRefresher refresher, params Guid[] guidIds)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");

            guidIds.ForEach(x => InvokeDispatchMethod(servers, refresher, MessageType.RefreshById, x));
            
        }

        public void PerformRefreshAll(IEnumerable<IServerRegistration> servers, ICacheRefresher refresher)
        {
            InvokeDispatchMethod(servers, refresher, MessageType.RefreshAll, null);
        }

        private void InvokeMethodOnRefresherInstance(ICacheRefresher refresher, MessageType dispatchType, object id)
        {
            if (refresher == null) throw new ArgumentNullException("refresher");

            //if we are not, then just invoke the call on the cache refresher
            switch (dispatchType)
            {
                case MessageType.RefreshAll:
                    refresher.RefreshAll();
                    break;
                case MessageType.RefreshById:
                    if (id is int)
                    {
                        refresher.Refresh((int)id);
                    }
                    else if (id is Guid)
                    {
                        refresher.Refresh((Guid) id);
                    }
                    else
                    {
                        throw new InvalidOperationException("The id must be either an int or a Guid");
                    }
                    
                    break;
                case MessageType.RemoveById:
                    refresher.Remove((int)id);
                    break;
            }
        }

        private void InvokeDispatchMethod(
            IEnumerable<IServerRegistration> servers, 
            ICacheRefresher refresher, 
            MessageType dispatchType, 
            object id)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");
            if (!(id is int) && (!(id is Guid))) throw new ArgumentException("The id must be either an int or a Guid");

            //Now, check if we are using Distrubuted calls. If there are no servers in the list then we
            // can definitely not distribute.
            if (!_useDistributedCalls || !servers.Any())
            {
                //if we are not, then just invoke the call on the cache refresher
                InvokeMethodOnRefresherInstance(refresher, dispatchType, id);
                return;
            }

            //We are using distributed calls, so lets make them...
            try
            {
                using (var cacheRefresher = new ServerSyncWebServiceClient())
                {
                    var asyncResultsList = new List<IAsyncResult>();

                    LogStartDispatch();

                    var nodes = servers;
                    // Go through each configured node submitting a request asynchronously
                    foreach (var n in nodes)
                    {
                        //set the server address
                        cacheRefresher.Url = n.ServerAddress;

                        // Add the returned WaitHandle to the list for later checking
                        switch (dispatchType)
                        {
                            case MessageType.RefreshAll:
                                asyncResultsList.Add(
                                    cacheRefresher.BeginRefreshAll(
                                        refresher.UniqueIdentifier, _login, _password, null, null));
                                break;
                            case MessageType.RefreshById:
                                IAsyncResult result;
                                if (id is int)
                                {
                                    result = cacheRefresher.BeginRefreshById(refresher.UniqueIdentifier, (int) id, _login, _password, null, null);
                                }
                                else
                                {
                                    result = cacheRefresher.BeginRefreshByGuid(refresher.UniqueIdentifier, (Guid)id, _login, _password, null, null);
                                }
                                asyncResultsList.Add(result);
                                break;                            
                            case MessageType.RemoveById:
                                asyncResultsList.Add(
                                    cacheRefresher.BeginRemoveById(
                                        refresher.UniqueIdentifier, (int)id, _login, _password, null, null));
                                break;
                        }
                    }

                    List<WaitHandle> waitHandlesList;
                    var asyncResults = GetAsyncResults(asyncResultsList, out waitHandlesList);

                    var errorCount = 0;

                    // Once for each WaitHandle that we have, wait for a response and log it
                    // We're previously submitted all these requests effectively in parallel and will now retrieve responses on a FIFO basis
                    foreach (var t in asyncResults)
                    {
                        var handleIndex = WaitHandle.WaitAny(waitHandlesList.ToArray(), TimeSpan.FromSeconds(15));

                        try
                        {
                            // Find out if the call succeeded
                            switch (dispatchType)
                            {
                                case MessageType.RefreshAll:
                                    cacheRefresher.EndRefreshAll(t);
                                    break;
                                case MessageType.RefreshById:
                                    if (id is int)
                                    {
                                        cacheRefresher.EndRefreshById(t);
                                    }
                                    else
                                    {
                                        cacheRefresher.EndRefreshByGuid(t);
                                    }
                                    break;
                                case MessageType.RemoveById:
                                    cacheRefresher.EndRemoveById(t);
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

        private IEnumerable<IAsyncResult> GetAsyncResults(List<IAsyncResult> asyncResultsList, out List<WaitHandle> waitHandlesList)
        {
            var asyncResults = asyncResultsList.ToArray();
            waitHandlesList = new List<WaitHandle>();
            foreach (var asyncResult in asyncResults)
            {
                waitHandlesList.Add(asyncResult.AsyncWaitHandle);
            }
            return asyncResults;
        }

        private void LogDispatchBatchError(Exception ee)
        {
            LogHelper.Error<DefaultServerMessenger>("Error refreshing distributed list", ee);
        }

        private void LogDispatchBatchResult(int errorCount)
        {
            LogHelper.Debug<DefaultServerMessenger>(string.Format("Distributed server push completed with {0} nodes reporting an error", errorCount == 0 ? "no" : errorCount.ToString(CultureInfo.InvariantCulture)));
        }

        private void LogDispatchNodeError(Exception ex)
        {
            LogHelper.Error<DefaultServerMessenger>("Error refreshing a node in the distributed list", ex);
        }

        private void LogDispatchNodeError(WebException ex)
        {
            string url = (ex.Response != null) ? ex.Response.ResponseUri.ToString() : "invalid url (responseUri null)";
            LogHelper.Error<DefaultServerMessenger>("Error refreshing a node in the distributed list, URI attempted: " + url, ex);
        }
        
        private void LogStartDispatch()
        {
            LogHelper.Info<DefaultServerMessenger>("Submitting calls to distributed servers");
        }

    }
}