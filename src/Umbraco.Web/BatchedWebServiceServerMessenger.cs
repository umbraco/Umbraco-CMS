using System;
using System.Collections.Generic;
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
            var httpContext = UmbracoContext.Current == null ? null : UmbracoContext.Current.HttpContext;
            if (httpContext == null)
            {
                if (ensureHttpContext)
                    throw new NotSupportedException("Cannot execute without a valid/current UmbracoContext with an HttpContext assigned.");
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
