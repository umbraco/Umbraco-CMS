using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using umbraco.interfaces;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// An <see cref="IServerMessenger"/> that works by messaging servers via web services.
    /// </summary>
    /// <remarks>
    /// this messenger sends ALL instructions to ALL servers, including the local server.
    /// the CacheRefresher web service will run ALL instructions, so there may be duplicated,
    /// except for "bulk" refresh, where it excludes those coming from the local server
    /// </remarks>        
    //
    // TODO see Message() method: stop sending to local server!
    // just need to figure out WebServerUtility permissions issues, if any
    //
    internal class WebServiceServerMessenger : ServerMessengerBase
    {
        private readonly Func<Tuple<string, string>> _getLoginAndPassword;
        private volatile bool _hasLoginAndPassword;
        private readonly object _locker = new object();

        protected string Login { get; private set; }
        protected string Password{ get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceServerMessenger"/> class.
        /// </summary>
        /// <remarks>Distribution is disabled.</remarks>
        internal WebServiceServerMessenger()
            : base(false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceServerMessenger"/> class with a login and a password.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <param name="password">The password.</param>
        /// <remarks>Distribution will be enabled based on the umbraco config setting.</remarks>
        internal WebServiceServerMessenger(string login, string password)
            : this(login, password, UmbracoConfig.For.UmbracoSettings().DistributedCall.Enabled)
        {            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceServerMessenger"/> class with a login and a password
        /// and a value indicating whether distribution is enabled.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <param name="password">The password.</param>
        /// <param name="distributedEnabled">A value indicating whether distribution is enabled.</param>
        internal WebServiceServerMessenger(string login, string password, bool distributedEnabled)
            : base(distributedEnabled)
        {
            if (login == null) throw new ArgumentNullException("login");
            if (password == null) throw new ArgumentNullException("password");

            Login = login;
            Password = password;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceServerMessenger"/> with a function providing
        /// a login and a password.
        /// </summary>
        /// <param name="getLoginAndPassword">A function providing a login and a password.</param>
        /// <remarks>Distribution will be enabled based on the umbraco config setting.</remarks>
        public WebServiceServerMessenger(Func<Tuple<string, string>> getLoginAndPassword)
            : base(false) // value will be overriden by EnsureUserAndPassword
        {
            _getLoginAndPassword = getLoginAndPassword;
        }

        // lazy-get the login, password, and distributed setting
        protected void EnsureLoginAndPassword()
        {
            if (_hasLoginAndPassword || _getLoginAndPassword == null) return;

            lock (_locker)
            {
                if (_hasLoginAndPassword) return;
                _hasLoginAndPassword = true;

                try
                {
                    var result = _getLoginAndPassword();
                    if (result == null)
                    {
                        Login = null;
                        Password = null;
                        DistributedEnabled = false;
                    }
                    else
                    {
                        Login = result.Item1;
                        Password = result.Item2;
                        DistributedEnabled = UmbracoConfig.For.UmbracoSettings().DistributedCall.Enabled;    
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error<WebServiceServerMessenger>("Could not resolve username/password delegate, server distribution will be disabled", ex);
                    Login = null;
                    Password = null;
                    DistributedEnabled = false;
                }
            }
        }

        // this exists only for legacy reasons - we should just pass the server identity un-hashed
        public static string GetCurrentServerHash()
        {
            if (SystemUtilities.GetCurrentTrustLevel() != System.Web.AspNetHostingPermissionLevel.Unrestricted)
                throw new NotSupportedException("FullTrust ASP.NET permission level is required.");
            return GetServerHash(NetworkHelper.MachineName, System.Web.HttpRuntime.AppDomainAppId);
        }

        public static string GetServerHash(string machineName, string appDomainAppId)
        {
            var hasher = new HashCodeCombiner();
            hasher.AddCaseInsensitiveString(appDomainAppId);
            hasher.AddCaseInsensitiveString(machineName);
            return hasher.GetCombinedHashCode();
        }

        protected override bool RequiresDistributed(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, MessageType messageType)
        {
            EnsureLoginAndPassword();
            return base.RequiresDistributed(servers, refresher, messageType);
        }

        protected override void DeliverRemote(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, MessageType messageType, IEnumerable<object> ids = null, string json = null)
        {
            var idsA = ids == null ? null : ids.ToArray();

            Type arrayType;
            if (GetArrayType(idsA, out arrayType) == false)
                throw new ArgumentException("All items must be of the same type, either int or Guid.", "ids");

            Message(servers, refresher, messageType, idsA, arrayType, json);
        }

        protected virtual void Message(
            IEnumerable<IServerAddress> servers,
            ICacheRefresher refresher,
            MessageType messageType, 
            IEnumerable<object> ids = null,
            Type idArrayType = null,
            string jsonPayload = null)
        {
            LogHelper.Debug<WebServiceServerMessenger>(
                "Performing distributed call for {0}/{1} on servers ({2}), ids: {3}, json: {4}",
                refresher.GetType,
                () => messageType,
                () => string.Join(";", servers.Select(x => x.ToString())),
                () => ids == null ? "" : string.Join(";", ids.Select(x => x.ToString())),
                () => jsonPayload ?? "");

            try
            {
                // NOTE: we are messaging ALL servers including the local server
                // at the moment, the web service,
                //  for bulk (batched) checks the origin and does NOT process the instructions again
                //  for anything else, processes the instructions again (but we don't use this anymore, batched is the default)
                // TODO: see WebServerHelper, could remove local server from the list of servers

                // the default server messenger uses http requests
                using (var client = new ServerSyncWebServiceClient())
                {
                    var asyncResults = new List<IAsyncResult>();

                    LogStartDispatch();

                    // go through each configured node submitting a request asynchronously
                    // NOTE: 'asynchronously' in this case does not mean that it will continue while we give the page back to the user!
                    foreach (var n in servers)
                    {
                        // set the server address
                        client.Url = n.ServerAddress;

                        // add the returned WaitHandle to the list for later checking
                        switch (messageType)
                        {
                            case MessageType.RefreshByJson:
                                asyncResults.Add(client.BeginRefreshByJson(refresher.UniqueIdentifier, jsonPayload, Login, Password, null, null));
                                break;

                            case MessageType.RefreshAll:
                                asyncResults.Add(client.BeginRefreshAll(refresher.UniqueIdentifier, Login, Password, null, null));
                                break;

                            case MessageType.RefreshById:
                                if (idArrayType == null)
                                    throw new InvalidOperationException("Cannot refresh by id if the idArrayType is null.");

                                if (idArrayType == typeof(int))
                                {
                                    // bulk of ints is supported
                                    var json = JsonConvert.SerializeObject(ids.Cast<int>().ToArray());
                                    var result = client.BeginRefreshByIds(refresher.UniqueIdentifier, json, Login, Password, null, null);
                                    asyncResults.Add(result);
                                }
                                else // must be guids
                                {
                                    // bulk of guids is not supported, iterate
                                    asyncResults.AddRange(ids.Select(i => 
                                        client.BeginRefreshByGuid(refresher.UniqueIdentifier, (Guid)i, Login, Password, null, null)));
                                }

                                break;
                            case MessageType.RemoveById:
                                if (idArrayType == null)
                                    throw new InvalidOperationException("Cannot remove by id if the idArrayType is null.");

                                // must be ints
                                asyncResults.AddRange(ids.Select(i => 
                                    client.BeginRemoveById(refresher.UniqueIdentifier, (int)i, Login, Password, null, null)));
                                break;
                        }
                    }

                    // wait for all requests to complete
                    var waitHandles = asyncResults.Select(x => x.AsyncWaitHandle);
                    WaitHandle.WaitAll(waitHandles.ToArray());

                    // handle results
                    var errorCount = 0;
                    foreach (var asyncResult in asyncResults)
                    {
                        try
                        {
                            switch (messageType)
                            {
                                case MessageType.RefreshByJson:
                                    client.EndRefreshByJson(asyncResult);
                                    break;

                                case MessageType.RefreshAll:
                                    client.EndRefreshAll(asyncResult);
                                    break;

                                case MessageType.RefreshById:
                                    if (idArrayType == typeof(int))
                                        client.EndRefreshById(asyncResult);
                                    else
                                        client.EndRefreshByGuid(asyncResult);
                                    break;

                                case MessageType.RemoveById:
                                    client.EndRemoveById(asyncResult);
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

        protected virtual void Message(IEnumerable<RefreshInstructionEnvelope> envelopes)
        {
            var envelopesA = envelopes.ToArray();
            var servers = envelopesA.SelectMany(x => x.Servers).Distinct();

            try
            {
                // NOTE: we are messaging ALL servers including the local server
                // at the moment, the web service,
                //  for bulk (batched) checks the origin and does NOT process the instructions again
                //  for anything else, processes the instructions again (but we don't use this anymore, batched is the default)
                // TODO: see WebServerHelper, could remove local server from the list of servers

                using (var client = new ServerSyncWebServiceClient())
                {
                    var asyncResults = new List<IAsyncResult>();

                    LogStartDispatch();

                    // go through each configured node submitting a request asynchronously
                    // NOTE: 'asynchronously' in this case does not mean that it will continue while we give the page back to the user!
                    foreach (var server in servers)
                    {
                        // set the server address
                        client.Url = server.ServerAddress;

                        var serverInstructions = envelopesA
                            .Where(x => x.Servers.Contains(server))
                            .SelectMany(x => x.Instructions)
                            .Distinct() // only execute distinct instructions - no sense in running the same one.
                            .ToArray();

                        asyncResults.Add(
                            client.BeginBulkRefresh(
                                serverInstructions,
                                GetCurrentServerHash(),
                                Login, Password, null, null));
                    }

                    // wait for all requests to complete
                    var waitHandles = asyncResults.Select(x => x.AsyncWaitHandle).ToArray();
                    WaitHandle.WaitAll(waitHandles.ToArray());

                    // handle results
                    var errorCount = 0;
                    foreach (var asyncResult in asyncResults)
                    {
                        try
                        {
                            client.EndBulkRefresh(asyncResult);
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

        #region Logging

        private static void LogDispatchBatchError(Exception ee)
        {
            LogHelper.Error<WebServiceServerMessenger>("Error refreshing distributed list", ee);
        }

        private static void LogDispatchBatchResult(int errorCount)
        {
            LogHelper.Debug<WebServiceServerMessenger>(string.Format("Distributed server push completed with {0} nodes reporting an error", errorCount == 0 ? "no" : errorCount.ToString(CultureInfo.InvariantCulture)));
        }

        private static void LogDispatchNodeError(Exception ex)
        {
            LogHelper.Error<WebServiceServerMessenger>("Error refreshing a node in the distributed list", ex);
        }

        private static void LogDispatchNodeError(WebException ex)
        {
            string url = (ex.Response != null) ? ex.Response.ResponseUri.ToString() : "invalid url (responseUri null)";
            LogHelper.Error<WebServiceServerMessenger>("Error refreshing a node in the distributed list, URI attempted: " + url, ex);
        }
        
        private static void LogStartDispatch()
        {
            LogHelper.Info<WebServiceServerMessenger>("Submitting calls to distributed servers");
        }

        #endregion
    }
}