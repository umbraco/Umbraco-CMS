using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
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
        private bool _useDistributedCalls;

        protected string Login { get; private set; }
        protected string Password{ get; private set; }

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
            : this(login, password, UmbracoConfig.For.UmbracoSettings().DistributedCall.Enabled)
        {            
        }

        /// <summary>
        /// Specifies the username/password and whether or not to use distributed calls
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <param name="useDistributedCalls"></param>
        internal DefaultServerMessenger(string login, string password, bool useDistributedCalls)
        {
            if (login == null) throw new ArgumentNullException("login");
            if (password == null) throw new ArgumentNullException("password");

            _useDistributedCalls = useDistributedCalls;
            Login = login;
            Password = password;
        }

        /// <summary>
        /// Allows to set a lazy delegate to resolve the username/password
        /// </summary>
        /// <param name="getUserNamePasswordDelegate"></param>
        public DefaultServerMessenger(Func<Tuple<string, string>> getUserNamePasswordDelegate)
        {
            _getUserNamePasswordDelegate = getUserNamePasswordDelegate;
        }

        public void PerformRefresh(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, string jsonPayload)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");
            if (jsonPayload == null) throw new ArgumentNullException("jsonPayload");

            MessageSeversForIdsOrJson(servers, refresher, MessageType.RefreshByJson, jsonPayload: jsonPayload);
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

            MessageSeversForIdsOrJson(servers, refresher, MessageType.RemoveById, numericIds.Cast<object>());
        }

        public void PerformRefresh(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, params int[] numericIds)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");

            MessageSeversForIdsOrJson(servers, refresher, MessageType.RefreshById, numericIds.Cast<object>());
        }

        public void PerformRefresh(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, params Guid[] guidIds)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");

            MessageSeversForIdsOrJson(servers, refresher, MessageType.RefreshById, guidIds.Cast<object>());
        }

        public void PerformRefreshAll(IEnumerable<IServerAddress> servers, ICacheRefresher refresher)
        {
            MessageSeversForIdsOrJson(servers, refresher, MessageType.RefreshAll, Enumerable.Empty<object>().ToArray());
        }

        private void InvokeMethodOnRefresherInstance<T>(ICacheRefresher refresher, MessageType dispatchType, Func<T, object> getId, IEnumerable<T> instances)
        {
            if (refresher == null) throw new ArgumentNullException("refresher");

            LogHelper.Debug<DefaultServerMessenger>("Invoking refresher {0} on single server instance, message type {1}",
                                                    () => refresher.GetType(),
                                                    () => dispatchType);

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
                                Login = null;
                                Password = null;
                                _useDistributedCalls = false;
                            }
                            else
                            {
                                Login = result.Item1;
                                Password = result.Item2;
                                _useDistributedCalls = UmbracoConfig.For.UmbracoSettings().DistributedCall.Enabled;    
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error<DefaultServerMessenger>("Could not resolve username/password delegate, server distribution will be disabled", ex);
                            Login = null;
                            Password = null;
                            _useDistributedCalls = false;
                        }
                    }
                }
            }
        }

        protected void InvokeMethodOnRefresherInstance(ICacheRefresher refresher, MessageType dispatchType, IEnumerable<object> ids = null, string jsonPayload = null)
        {
            if (refresher == null) throw new ArgumentNullException("refresher");

            LogHelper.Debug<DefaultServerMessenger>("Invoking refresher {0} on single server instance, message type {1}",
                                                    () => refresher.GetType(),
                                                    () => dispatchType);

            //if it is a refresh all we'll do it here since ids will be null or empty
            if (dispatchType == MessageType.RefreshAll)
            {
                refresher.RefreshAll();
            }
            else
            {
                if (ids != null)
                {
                    foreach (var id in ids)
                    {
                        //if we are not, then just invoke the call on the cache refresher
                        switch (dispatchType)
                        {
                            case MessageType.RefreshById:
                                if (id is int)
                                {
                                    refresher.Refresh((int) id);
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
                                refresher.Remove((int) id);
                                break;
                        }
                    }
                }
                else
                {
                    //we can only proceed if the cache refresher is IJsonCacheRefresher!
                    var jsonRefresher = refresher as IJsonCacheRefresher;
                    if (jsonRefresher == null)
                    {
                        throw new InvalidOperationException("The cache refresher " + refresher.GetType() + " is not of type " + typeof(IJsonCacheRefresher));
                    }

                    //if we are not, then just invoke the call on the cache refresher
                    jsonRefresher.Refresh(jsonPayload);
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
            MessageSeversForIdsOrJson(servers, refresher, dispatchType, instances.Select(getId));
        }

        private void MessageSeversForIdsOrJson(
            IEnumerable<IServerAddress> servers,
            ICacheRefresher refresher,
            MessageType dispatchType,
            IEnumerable<object> ids = null,
            string jsonPayload = null)
        {
            if (servers == null) throw new ArgumentNullException("servers");
            if (refresher == null) throw new ArgumentNullException("refresher");
            
            Type arrayType;
            if (!ValidateIdArray(ids, out arrayType))
            {
                throw new ArgumentException("The id must be either an int or a Guid");
            }

            EnsureLazyUsernamePasswordDelegateResolved();

            //Now, check if we are using Distrubuted calls. If there are no servers in the list then we
            // can definitely not distribute.
            if (!_useDistributedCalls || !servers.Any())
            {
                //if we are not, then just invoke the call on the cache refresher
                InvokeMethodOnRefresherInstance(refresher, dispatchType, ids, jsonPayload);
                return;
            }
            
            LogHelper.Debug<DefaultServerMessenger>(
                "Performing distributed call for refresher {0}, message type: {1}, servers: {2}, ids: {3}, json: {4}",
                refresher.GetType,
                () => dispatchType,
                () => string.Join(";", servers.Select(x => x.ToString())),
                () => ids == null ? "" : string.Join(";", ids.Select(x => x.ToString())),
                () => jsonPayload ?? "");

            PerformDistributedCall(servers, refresher, dispatchType, ids, arrayType, jsonPayload);
        }

        private bool ValidateIdArray(IEnumerable<object> ids, out Type arrayType)
        {
            arrayType = null;
            if (ids != null)
            {
                foreach (var id in ids)
                {
                    if (!(id is int) && (!(id is Guid)))
                        return false; //
                    if (arrayType == null)
                        arrayType = id.GetType();
                    if (arrayType != id.GetType())
                        throw new ArgumentException("The array must contain the same type of " + arrayType);
                }
            }
            return true;
        }

        protected virtual void PerformDistributedCall(
            IEnumerable<IServerAddress> servers,
            ICacheRefresher refresher,
            MessageType dispatchType, 
            IEnumerable<object> ids = null,
            Type idArrayType = null,
            string jsonPayload = null)
        {
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
                    //NOTE: 'asynchronously' in this case does not mean that it will continue while we give the page back to the user!
                    foreach (var n in servers)
                    {
                        //set the server address
                        cacheRefresher.Url = n.ServerAddress;

                        // Add the returned WaitHandle to the list for later checking
                        switch (dispatchType)
                        {
                            case MessageType.RefreshByJson:
                                asyncResultsList.Add(
                                    cacheRefresher.BeginRefreshByJson(
                                        refresher.UniqueIdentifier, jsonPayload, Login, Password, null, null));
                                break;
                            case MessageType.RefreshAll:
                                asyncResultsList.Add(
                                    cacheRefresher.BeginRefreshAll(
                                        refresher.UniqueIdentifier, Login, Password, null, null));
                                break;
                            case MessageType.RefreshById:
                                if (idArrayType == null)
                                {
                                    throw new InvalidOperationException("Cannot refresh by id if the idArrayType is null");
                                }

                                if (idArrayType == typeof(int))
                                {
                                    var serializer = new JavaScriptSerializer();
                                    var jsonIds = serializer.Serialize(ids.Cast<int>().ToArray());
                                    //we support bulk loading of Integers 
                                    var result = cacheRefresher.BeginRefreshByIds(refresher.UniqueIdentifier, jsonIds, Login, Password, null, null);
                                    asyncResultsList.Add(result);
                                }
                                else
                                {
                                    //we don't currently support bulk loading of GUIDs (not even sure if we have any Guid ICacheRefreshers)
                                    //so we'll just iterate
                                    asyncResultsList.AddRange(
                                        ids.Select(i => cacheRefresher.BeginRefreshByGuid(
                                            refresher.UniqueIdentifier, (Guid)i, Login, Password, null, null)));
                                }

                                break;
                            case MessageType.RemoveById:
                                //we don't currently support bulk removing so we'll iterate
                                asyncResultsList.AddRange(
                                        ids.Select(i => cacheRefresher.BeginRemoveById(
                                            refresher.UniqueIdentifier, (int)i, Login, Password, null, null)));
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
                                case MessageType.RefreshByJson:
                                    cacheRefresher.EndRefreshByJson(t);
                                    break;
                                case MessageType.RefreshAll:
                                    cacheRefresher.EndRefreshAll(t);
                                    break;
                                case MessageType.RefreshById:
                                    if (idArrayType == null)
                                    {
                                        throw new InvalidOperationException("Cannot refresh by id if the idArrayType is null");
                                    }

                                    if (idArrayType == typeof(int))
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

        internal IEnumerable<IAsyncResult> GetAsyncResults(List<IAsyncResult> asyncResultsList, out List<WaitHandle> waitHandlesList)
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