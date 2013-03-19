using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Script.Serialization;
using Umbraco.Core.Cache;
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
        private readonly Func<Tuple<string, string>> _getUserNamePasswordDelegate;
        private volatile bool _hasResolvedDelegate = false;
        private readonly object _locker = new object();
        private string _login;
        private string _password;
        private bool _useDistributedCalls;

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
            if (login == null) throw new ArgumentNullException("login");
            if (password == null) throw new ArgumentNullException("password");

            _useDistributedCalls = UmbracoSettings.UseDistributedCalls;
            _login = login;
            _password = password;
        }

        /// <summary>
        /// Allows to set a lazy delegate to resolve the username/password
        /// </summary>
        /// <param name="getUserNamePasswordDelegate"></param>
        public DefaultServerMessenger(Func<Tuple<string, string>> getUserNamePasswordDelegate)
        {
            _getUserNamePasswordDelegate = getUserNamePasswordDelegate;
        }

        public void PerformRefresh<T>(IEnumerable<IServerAddress> servers, ICacheRefresher refresher,Func<T, int> getNumericId, params T[] instances)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");

            //copy local
            var idGetter = getNumericId;

            MessageSeversForManyObjects(servers, refresher, MessageType.RefreshById,
                x => idGetter(x), 
                instances);
        }

        public void PerformRefresh<T>(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, Func<T, Guid> getGuidId, params T[] instances)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");

            //copy local
            var idGetter = getGuidId;

            MessageSeversForManyObjects(servers, refresher, MessageType.RefreshById,
                x => idGetter(x),
                instances);
        }

        public void PerformRemove<T>(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");

            //copy local
            var idGetter = getNumericId;

            MessageSeversForManyObjects(servers, refresher, MessageType.RemoveById,
                x => idGetter(x),
                instances);
        }

        public void PerformRemove(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, params int[] numericIds)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");

            MessageSeversForManyIds(servers, refresher, MessageType.RemoveById, numericIds.Cast<object>());
        }

        public void PerformRefresh(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, params int[] numericIds)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");

            MessageSeversForManyIds(servers, refresher, MessageType.RefreshById, numericIds.Cast<object>());
        }

        public void PerformRefresh(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, params Guid[] guidIds)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");

            MessageSeversForManyIds(servers, refresher, MessageType.RefreshById, guidIds.Cast<object>());
        }

        public void PerformRefreshAll(IEnumerable<IServerAddress> servers, ICacheRefresher refresher)
        {
            MessageSeversForManyIds(servers, refresher, MessageType.RefreshAll, Enumerable.Empty<object>().ToArray());
        }

        private void InvokeMethodOnRefresherInstance<T>(ICacheRefresher refresher, MessageType dispatchType, Func<T, object> getId, IEnumerable<T> instances)
        {
            if (refresher == null) throw new ArgumentNullException("refresher");

            var stronglyTypedRefresher = refresher as ICacheRefresher<T>;
            
            foreach (var instance in instances)
            {
                //if we are not, then just invoke the call on the cache refresher
                switch (dispatchType)
                {
                    case MessageType.RefreshAll:
                        refresher.RefreshAll();
                        break;
                    case MessageType.RefreshById:
                        if (stronglyTypedRefresher != null)
                        {
                            stronglyTypedRefresher.Refresh(instance);
                        }
                        else
                        {
                            var id = getId(instance);
                            if (id is int)
                            {
                                refresher.Refresh((int)id);
                            }
                            else if (id is Guid)
                            {
                                refresher.Refresh((Guid)id);
                            }
                            else
                            {
                                throw new InvalidOperationException("The id must be either an int or a Guid");
                            }
                        }
                        break;
                    case MessageType.RemoveById:
                        if (stronglyTypedRefresher != null)
                        {
                            stronglyTypedRefresher.Remove(instance);
                        }
                        else
                        {
                            var id = getId(instance);
                            refresher.Refresh((int)id);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// If we are instantiated with a lazy delegate to get the username/password, we'll resolve it here
        /// </summary>
        private void EnsureLazyUsernamePasswordDelegateResolved()
        {
            if (!_hasResolvedDelegate && _getUserNamePasswordDelegate != null)
            {
                lock (_locker)
                {
                    if (!_hasResolvedDelegate)
                    {
                        _hasResolvedDelegate = true; //set flag

                        try
                        {
                            var result = _getUserNamePasswordDelegate();
                            if (result == null)
                            {
                                _login = null;
                                _password = null;
                                _useDistributedCalls = false;
                            }
                            else
                            {
                                _login = result.Item1;
                                _password = result.Item2;
                                _useDistributedCalls = UmbracoSettings.UseDistributedCalls;    
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error<DefaultServerMessenger>("Could not resolve username/password delegate, server distribution will be disabled", ex);
                            _login = null;
                            _password = null;
                            _useDistributedCalls = false;
                        }
                    }
                }
            }
        }

        private void InvokeMethodOnRefresherInstance(ICacheRefresher refresher, MessageType dispatchType, IEnumerable<object> ids)
        {
            if (refresher == null) throw new ArgumentNullException("refresher");

            //if it is a refresh all we'll do it here since ids will be null or empty
            if (dispatchType == MessageType.RefreshAll)
            {
                refresher.RefreshAll();
            }
            else
            {
                foreach (var id in ids)
                {
                    //if we are not, then just invoke the call on the cache refresher
                    switch (dispatchType)
                    {                        
                        case MessageType.RefreshById:
                            if (id is int)
                            {
                                refresher.Refresh((int)id);
                            }
                            else if (id is Guid)
                            {
                                refresher.Refresh((Guid)id);
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
            }

            
            
        }

        private void MessageSeversForManyObjects<T>(
            IEnumerable<IServerAddress> servers,
            ICacheRefresher refresher,
            MessageType dispatchType,
            Func<T, object> getId,
            IEnumerable<T> instances)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");

            EnsureLazyUsernamePasswordDelegateResolved();

            //Now, check if we are using Distrubuted calls. If there are no servers in the list then we
            // can definitely not distribute.
            if (!_useDistributedCalls || !servers.Any())
            {
                //if we are not, then just invoke the call on the cache refresher
                InvokeMethodOnRefresherInstance(refresher, dispatchType, getId, instances);
                return;
            }

            //if we are distributing calls then we'll need to do it by id
            MessageSeversForManyIds(servers, refresher, dispatchType, instances.Select(getId));
        }

        private void MessageSeversForManyIds(
            IEnumerable<IServerAddress> servers,
            ICacheRefresher refresher,
            MessageType dispatchType,
            IEnumerable<object> ids)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");
            Type arrayType = null;
            foreach (var id in ids)
            {
                if (!(id is int) && (!(id is Guid)))
                    throw new ArgumentException("The id must be either an int or a Guid");
                if (arrayType == null)
                    arrayType = id.GetType();
                if (arrayType != id.GetType())
                    throw new ArgumentException("The array must contain the same type of " + arrayType);
            }

            //Now, check if we are using Distrubuted calls. If there are no servers in the list then we
            // can definitely not distribute.
            if (!_useDistributedCalls || !servers.Any())
            {
                //if we are not, then just invoke the call on the cache refresher
                InvokeMethodOnRefresherInstance(refresher, dispatchType, ids);
                return;
            }

            //We are using distributed calls, so lets make them...
            try
            {

                //TODO: We should try to figure out the current server's address and if it matches any of the ones
                // in the ServerAddress list, then just refresh directly on this server and exclude that server address
                // from the list, this will save an internal request.

                using (var cacheRefresher = new ServerSyncWebServiceClient())
                {
                    var asyncResultsList = new List<IAsyncResult>();

                    LogStartDispatch();

                    // Go through each configured node submitting a request asynchronously
                    foreach (var n in servers)
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
                                if (arrayType == typeof(int))
                                {
                                    var serializer = new JavaScriptSerializer();
                                    var jsonIds = serializer.Serialize(ids.Cast<int>().ToArray());
                                    //we support bulk loading of Integers 
                                    var result = cacheRefresher.BeginRefreshByIds(refresher.UniqueIdentifier, jsonIds, _login, _password, null, null);
                                    asyncResultsList.Add(result);
                                }
                                else
                                {
                                    //we don't currently support bulk loading of GUIDs (not even sure if we have any Guid ICacheRefreshers)
                                    //so we'll just iterate
                                    asyncResultsList.AddRange(
                                        ids.Select(i => cacheRefresher.BeginRefreshByGuid(
                                            refresher.UniqueIdentifier, (Guid) i, _login, _password, null, null)));
                                }
                                
                                break;
                            case MessageType.RemoveById:
                                //we don't currently support bulk removing so we'll iterate
                                asyncResultsList.AddRange(
                                        ids.Select(i => cacheRefresher.BeginRemoveById(
                                            refresher.UniqueIdentifier, (int)i, _login, _password, null, null)));
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
                                    if (arrayType == typeof(int))
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