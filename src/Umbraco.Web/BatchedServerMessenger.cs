using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;
using umbraco.interfaces;

namespace Umbraco.Web
{
    internal class BatchedServerMessenger : DefaultServerMessenger
    {
        internal BatchedServerMessenger()
        {
            UmbracoModule.EndRequest += UmbracoModule_EndRequest;
        }

        internal BatchedServerMessenger(string login, string password) : base(login, password)
        {
            UmbracoModule.EndRequest += UmbracoModule_EndRequest;
        }

        internal BatchedServerMessenger(string login, string password, bool useDistributedCalls) : base(login, password, useDistributedCalls)
        {
            UmbracoModule.EndRequest += UmbracoModule_EndRequest;
        }

        public BatchedServerMessenger(Func<Tuple<string, string>> getUserNamePasswordDelegate) : base(getUserNamePasswordDelegate)
        {
            UmbracoModule.EndRequest += UmbracoModule_EndRequest;
        }

        void UmbracoModule_EndRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current == null)
            {
                return;
            }

            var items = HttpContext.Current.Items[typeof(BatchedServerMessenger).Name] as List<Message>;
            if (items != null)
            {
                var copied = new Message[items.Count];
                items.CopyTo(copied);
                //now set to null so it get's cleaned up on this request
                HttpContext.Current.Items[typeof (BatchedServerMessenger).Name] = null;

                SendMessages(copied);
            }
        }

        private class Message
        {
            public IEnumerable<IServerAddress> Servers { get; set; }
            public ICacheRefresher Refresher { get; set; }
            public MessageType DispatchType { get; set; }
            public IEnumerable<object> Ids { get; set; }
            public Type IdArrayType { get; set; }
            public string JsonPayload { get; set; }
        }

        /// <summary>
        /// We need to check if distributed calls are enabled, if they are we also want to make sure 
        /// that the current server's cache is updated internally in real time instead of at the end of
        /// the call. This is because things like the URL cache, etc... might need to be updated during
        /// the request that is making these calls.
        /// </summary>
        /// <param name="servers"></param>
        /// <param name="refresher"></param>
        /// <param name="dispatchType"></param>
        /// <param name="ids"></param>
        /// <param name="jsonPayload"></param>
        /// <remarks>
        /// See: http://issues.umbraco.org/issue/U4-2633#comment=67-15604
        /// </remarks>
        protected override void MessageSeversForIdsOrJson(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, MessageType dispatchType, IEnumerable<object> ids = null, string jsonPayload = null)
        {
            //do all the normal stuff
            base.MessageSeversForIdsOrJson(servers, refresher, dispatchType, ids, jsonPayload);

            //Now, check if we are using Distrubuted calls
            if (UseDistributedCalls && servers.Any())
            {
                //invoke on the current server - we will basically be double cache refreshing for the calling
                // server but that just needs to be done currently, see the link above for details.
                InvokeMethodOnRefresherInstance(refresher, dispatchType, ids, jsonPayload);
            }
        }

        /// <summary>
        /// This adds the call to batched list
        /// </summary>
        /// <param name="servers"></param>
        /// <param name="refresher"></param>
        /// <param name="dispatchType"></param>
        /// <param name="ids"></param>
        /// <param name="idArrayType"></param>
        /// <param name="jsonPayload"></param>
        protected override void PerformDistributedCall(
            IEnumerable<IServerAddress> servers,
            ICacheRefresher refresher,
            MessageType dispatchType,
            IEnumerable<object> ids = null,
            Type idArrayType = null,
            string jsonPayload = null)
        {

            //NOTE: we use UmbracoContext instead of HttpContext.Current because when some web methods run async, the 
            // HttpContext.Current is null but the UmbracoContext.Current won't be since we manually assign it.
            if (UmbracoContext.Current == null || UmbracoContext.Current.HttpContext == null)
            {
                throw new NotSupportedException("This messenger cannot execute without a valid/current UmbracoContext with an HttpContext assigned");
            }

            if (UmbracoContext.Current.HttpContext.Items[typeof(BatchedServerMessenger).Name] == null)
            {
                UmbracoContext.Current.HttpContext.Items[typeof(BatchedServerMessenger).Name] = new List<Message>();
            }
            var list = (List<Message>)UmbracoContext.Current.HttpContext.Items[typeof(BatchedServerMessenger).Name];

            list.Add(new Message
            {
                DispatchType = dispatchType,
                IdArrayType = idArrayType,
                Ids = ids,
                JsonPayload = jsonPayload,
                Refresher = refresher,
                Servers = servers
            });
        }

        private RefreshInstruction[] ConvertToInstruction(Message msg)
        {
            switch (msg.DispatchType)
            {
                case MessageType.RefreshAll:
                    return new[]
                        {
                            new RefreshInstruction
                            {
                                RefreshType = RefreshInstruction.RefreshMethodType.RefreshAll,
                                RefresherId = msg.Refresher.UniqueIdentifier
                            }
                        };
                case MessageType.RefreshById:
                    if (msg.IdArrayType == null)
                    {
                        throw new InvalidOperationException("Cannot refresh by id if the idArrayType is null");
                    }

                    if (msg.IdArrayType == typeof(int))
                    {
                        var serializer = new JavaScriptSerializer();
                        var jsonIds = serializer.Serialize(msg.Ids.Cast<int>().ToArray());

                        return new[]
                        {
                            new RefreshInstruction
                            {
                                JsonIds = jsonIds,
                                RefreshType = RefreshInstruction.RefreshMethodType.RefreshByIds,
                                RefresherId = msg.Refresher.UniqueIdentifier
                            }
                        };
                    }

                    return msg.Ids.Select(x => new RefreshInstruction
                    {
                        GuidId = (Guid)x,
                        RefreshType = RefreshInstruction.RefreshMethodType.RefreshById,
                        RefresherId = msg.Refresher.UniqueIdentifier
                    }).ToArray();

                case MessageType.RefreshByJson:
                    return new[]
                        {
                            new RefreshInstruction
                            {
                                RefreshType = RefreshInstruction.RefreshMethodType.RefreshByJson,
                                RefresherId = msg.Refresher.UniqueIdentifier,
                                JsonPayload = msg.JsonPayload
                            }
                        };
                case MessageType.RemoveById:
                    return msg.Ids.Select(x => new RefreshInstruction
                    {
                        IntId = (int)x,
                        RefreshType = RefreshInstruction.RefreshMethodType.RemoveById,
                        RefresherId = msg.Refresher.UniqueIdentifier
                    }).ToArray();
                case MessageType.RefreshByInstance:                    
                case MessageType.RemoveByInstance:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SendMessages(IEnumerable<Message> messages)
        {
            var batchedMsg = new List<Tuple<Message, RefreshInstruction[]>>();
            foreach (var msg in messages)
            {
                var instructions = ConvertToInstruction(msg);
                batchedMsg.Add(new Tuple<Message, RefreshInstruction[]>(msg, instructions));
            }

            var servers = batchedMsg.SelectMany(x => x.Item1.Servers).Distinct();
            
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
                    foreach (var server in servers)
                    {
                        //set the server address
                        cacheRefresher.Url = server.ServerAddress;

                        var instructions = batchedMsg
                            .Where(x => x.Item1.Servers.Contains(server))
                            .SelectMany(x => x.Item2)
                            //only execute distinct instructions - no sense in running the same one.
                            .Distinct()
                            .ToArray();

                        //Create a hash of the server name and the IIS app Id to send up so we don't double cache refresh the
                        // master server. 
                        //Fixes: http://issues.umbraco.org/issue/U4-5491
                        //NOTE: This will only work in full trust, in med trust, a double cache refresh is inevitable 
                        var hashedAppId = string.Empty;
                        if (SystemUtilities.GetCurrentTrustLevel() == AspNetHostingPermissionLevel.Unrestricted)
                        {
                            var hasher = new HashCodeCombiner();
                            hasher.AddCaseInsensitiveString(NetworkHelper.MachineName);
                            hasher.AddCaseInsensitiveString(HttpRuntime.AppDomainAppId);
                            hashedAppId = hasher.GetCombinedHashCode();
                        }
                        
                        asyncResultsList.Add(
                            cacheRefresher.BeginBulkRefresh(
                                instructions,
                                hashedAppId,
                                Login, Password, null, null));
                    }

                    var waitHandlesList = asyncResultsList.Select(x => x.AsyncWaitHandle).ToArray();
                    
                    var errorCount = 0;

                    //Wait for all requests to complete
                    WaitHandle.WaitAll(waitHandlesList.ToArray());
                    
                    foreach (var t in asyncResultsList)
                    {
                        //var handleIndex = WaitHandle.WaitAny(waitHandlesList.ToArray(), TimeSpan.FromSeconds(15));

                        try
                        {
                            cacheRefresher.EndBulkRefresh(t);
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

        private void LogDispatchBatchError(Exception ee)
        {
            LogHelper.Error<BatchedServerMessenger>("Error refreshing distributed list", ee);
        }

        private void LogDispatchBatchResult(int errorCount)
        {
            LogHelper.Debug<BatchedServerMessenger>(string.Format("Distributed server push completed with {0} nodes reporting an error", errorCount == 0 ? "no" : errorCount.ToString(CultureInfo.InvariantCulture)));
        }

        private void LogDispatchNodeError(Exception ex)
        {
            LogHelper.Error<BatchedServerMessenger>("Error refreshing a node in the distributed list", ex);
        }

        private void LogDispatchNodeError(WebException ex)
        {
            string url = (ex.Response != null) ? ex.Response.ResponseUri.ToString() : "invalid url (responseUri null)";
            LogHelper.Error<BatchedServerMessenger>("Error refreshing a node in the distributed list, URI attempted: " + url, ex);
        }

        private void LogStartDispatch()
        {
            LogHelper.Info<BatchedServerMessenger>("Submitting calls to distributed servers");
        }
    }
}
