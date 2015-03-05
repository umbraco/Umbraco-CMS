using System;
using System.Collections.Generic;
using Umbraco.Core.Sync;

namespace Umbraco.Web
{
    internal class BatchedWebServiceServerMessenger : Core.Sync.BatchedWebServiceServerMessenger
    {
        internal BatchedWebServiceServerMessenger()
            : base(GetBatch)
        {
            UmbracoModule.EndRequest += UmbracoModule_EndRequest;
        }

        internal BatchedWebServiceServerMessenger(string login, string password) 
            : base(login, password, GetBatch)
        {
            UmbracoModule.EndRequest += UmbracoModule_EndRequest;
        }

        internal BatchedWebServiceServerMessenger(string login, string password, bool useDistributedCalls) 
            : base(login, password, useDistributedCalls, GetBatch)
        {
            UmbracoModule.EndRequest += UmbracoModule_EndRequest;
        }

        public BatchedWebServiceServerMessenger(Func<Tuple<string, string>> getLoginAndPassword)
            : base(getLoginAndPassword, GetBatch)
        {
            UmbracoModule.EndRequest += UmbracoModule_EndRequest;
        }

        private static ICollection<RefreshInstructionEnvelope> GetBatch(bool ensure)
        {
            var httpContext = UmbracoContext.Current == null ? null : UmbracoContext.Current.HttpContext;
            if (httpContext == null)
            {
                if (ensure)
                    throw new NotSupportedException("Cannot execute without a valid/current UmbracoContext with an HttpContext assigned.");
                return null;
            }

            var key = typeof(BatchedWebServiceServerMessenger).Name;

            // no thread-safety here because it'll run in only 1 thread (request) at a time
            var batch = (ICollection<RefreshInstructionEnvelope>)httpContext.Items[key];
            if (batch == null && ensure)
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
