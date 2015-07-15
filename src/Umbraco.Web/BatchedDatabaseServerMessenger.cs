using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using umbraco.interfaces;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Sync;
using Umbraco.Web.Routing;

namespace Umbraco.Web
{
    /// <summary>
    /// An <see cref="IServerMessenger"/> implementation that works by storing messages in the database.
    /// </summary>
    /// <remarks>
    /// This binds to appropriate umbraco events in order to trigger the Boot(), Sync() & FlushBatch() calls
    /// </remarks>
    public class BatchedDatabaseServerMessenger : Core.Sync.DatabaseServerMessenger
    {
        public BatchedDatabaseServerMessenger(ApplicationContext appContext, bool enableDistCalls, DatabaseServerMessengerOptions options)
            : base(appContext, enableDistCalls, options)
        {
            UmbracoApplicationBase.ApplicationStarted += Application_Started;
            UmbracoModule.EndRequest += UmbracoModule_EndRequest;
            UmbracoModule.RouteAttempt += UmbracoModule_RouteAttempt;
        }

        private void Application_Started(object sender, EventArgs eventArgs)
        {
            if (ApplicationContext.IsConfigured  == false 
                || ApplicationContext.DatabaseContext.IsDatabaseConfigured  == false
                || ApplicationContext.DatabaseContext.CanConnect == false)

                LogHelper.Warn<BatchedDatabaseServerMessenger>("The app is not configured or cannot connect to the database, this server cannot be initialized with " 
                    + typeof(BatchedDatabaseServerMessenger) + ", distributed calls will not be enabled for this server");

            // because .ApplicationStarted triggers only once, this is thread-safe
            Boot();
        }

        private void UmbracoModule_RouteAttempt(object sender, RoutableAttemptEventArgs e)
        {
            switch (e.Outcome)
            {
                case EnsureRoutableOutcome.IsRoutable:
                    Sync();
                    break;
                case EnsureRoutableOutcome.NotDocumentRequest:
                    //so it's not a document request, we'll check if it's a back office request
                    if (e.HttpContext.Request.Url.IsBackOfficeRequest(HttpRuntime.AppDomainAppVirtualPath))
                    {
                        //it's a back office request, we should sync!
                        Sync();
                    }
                    break;
                //case EnsureRoutableOutcome.NotReady:
                //case EnsureRoutableOutcome.NotConfigured:
                //case EnsureRoutableOutcome.NoContent:
                //default:
                //    break;
            }
        }

        private void UmbracoModule_EndRequest(object sender, EventArgs e)
        {
            // will clear the batch - will remain in HttpContext though - that's ok
            FlushBatch();
        }

        protected override void DeliverRemote(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, MessageType messageType, IEnumerable<object> ids = null, string json = null)
        {
            var idsA = ids == null ? null : ids.ToArray();

            Type arrayType;
            if (GetArrayType(idsA, out arrayType) == false)
                throw new ArgumentException("All items must be of the same type, either int or Guid.", "ids");

            BatchMessage(servers, refresher, messageType, idsA, arrayType, json);
        }

        public void FlushBatch()
        {
            var batch = GetBatch(false);
            if (batch == null) return;

            var instructions = batch.SelectMany(x => x.Instructions).ToArray();
            batch.Clear();
            if (instructions.Length == 0) return;

            var dto = new CacheInstructionDto
            {
                UtcStamp = DateTime.UtcNow,
                Instructions = JsonConvert.SerializeObject(instructions, Formatting.None),
                OriginIdentity = LocalIdentity
            };

            ApplicationContext.DatabaseContext.Database.Insert(dto);
        }

        protected ICollection<RefreshInstructionEnvelope> GetBatch(bool ensureHttpContext)
        {
            var httpContext = UmbracoContext.Current == null ? null : UmbracoContext.Current.HttpContext;
            if (httpContext == null)
            {
                if (ensureHttpContext)
                    throw new NotSupportedException("Cannot execute without a valid/current UmbracoContext with an HttpContext assigned.");
                return null;
            }

            var key = typeof (BatchedDatabaseServerMessenger).Name;

            // no thread-safety here because it'll run in only 1 thread (request) at a time
            var batch = (ICollection<RefreshInstructionEnvelope>)httpContext.Items[key];
            if (batch == null && ensureHttpContext)
                httpContext.Items[key] = batch = new List<RefreshInstructionEnvelope>();
            return batch;
        }

        protected void BatchMessage(
            IEnumerable<IServerAddress> servers,
            ICacheRefresher refresher,
            MessageType messageType,
            IEnumerable<object> ids = null,
            Type idType = null,
            string json = null)
        {
            var batch = GetBatch(true);
            if (batch == null)
                throw new Exception("Failed to get a batch.");

            batch.Add(new RefreshInstructionEnvelope(servers, refresher,
                RefreshInstruction.GetInstructions(refresher, messageType, ids, idType, json)));
        }
    }
}