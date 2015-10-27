using System;
using System.Collections.Generic;
using System.Web;
using Umbraco.Core.Sync;

namespace Umbraco.Web
{
    /// <summary>
    /// An <see cref="IServerMessenger"/> that works by messaging servers via web services.
    /// </summary>
    /// <remarks>
    /// This binds to appropriate umbraco events in order to trigger the FlushBatch() calls
    /// </remarks>
    internal class BatchedWebServiceServerMessenger : Core.Sync.BatchedWebServiceServerMessenger
    {
        internal BatchedWebServiceServerMessenger()
            : base()
        {
            UmbracoModule.EndRequest += UmbracoModule_EndRequest;
        }

        internal BatchedWebServiceServerMessenger(string login, string password) 
            : base(login, password)
        {
            UmbracoModule.EndRequest += UmbracoModule_EndRequest;
        }

        internal BatchedWebServiceServerMessenger(string login, string password, bool useDistributedCalls) 
            : base(login, password, useDistributedCalls)
        {
            UmbracoModule.EndRequest += UmbracoModule_EndRequest;
        }

        public BatchedWebServiceServerMessenger(Func<Tuple<string, string>> getLoginAndPassword)
            : base(getLoginAndPassword)
        {
            UmbracoModule.EndRequest += UmbracoModule_EndRequest;
        }

        protected override ICollection<RefreshInstructionEnvelope> GetBatch(bool ensureHttpContext)
        {
            //try get the http context from the UmbracoContext, we do this because in the case we are launching an async
            // thread and we know that the cache refreshers will execute, we will ensure the UmbracoContext and therefore we
            // can get the http context from it
            var httpContext = (UmbracoContext.Current == null ? null : UmbracoContext.Current.HttpContext)
                //if this is null, it could be that an async thread is calling this method that we weren't aware of and the UmbracoContext
                // wasn't ensured at the beginning of the thread. We can try to see if the HttpContext.Current is available which might be 
                // the case if the asp.net synchronization context has kicked in
                ?? (HttpContext.Current == null ? null : new HttpContextWrapper(HttpContext.Current));

            if (httpContext == null)
            {
                if (ensureHttpContext)
                    throw new NotSupportedException("Cannot execute without a valid/current HttpContext assigned.");
                return null;
            }

            var key = typeof(BatchedWebServiceServerMessenger).Name;

            // no thread-safety here because it'll run in only 1 thread (request) at a time
            var batch = (ICollection<RefreshInstructionEnvelope>)httpContext.Items[key];
            if (batch == null && ensureHttpContext)
                httpContext.Items[key] = batch = new List<RefreshInstructionEnvelope>();
            return batch;
        }

        void UmbracoModule_EndRequest(object sender, EventArgs e)
        {
            FlushBatch();
        }

        protected override void ProcessBatch(RefreshInstructionEnvelope[] batch)
        {
            Message(batch);
        }
    }
}
