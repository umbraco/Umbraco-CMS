using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Sync;
using Umbraco.Web.Routing;
using Umbraco.Core.Logging;

namespace Umbraco.Web
{
    /// <summary>
    /// An <see cref="IServerMessenger"/> implementation that works by storing messages in the database.
    /// </summary>
    /// <remarks>
    /// This binds to appropriate umbraco events in order to trigger the Boot(), Sync() & FlushBatch() calls
    /// </remarks>
    public class BatchedDatabaseServerMessenger : DatabaseServerMessenger
    {
        public BatchedDatabaseServerMessenger(
            IRuntimeState runtime, DatabaseContext dbContext, ILogger logger, ProfilingLogger proflog,
            bool enableDistCalls, DatabaseServerMessengerOptions options)
            : base(runtime, dbContext, logger, proflog, enableDistCalls, options)
        { }

        // invoked by BatchedDatabaseServerMessengerStartup which is an ApplicationEventHandler
        // with default "ShouldExecute", so that method will run if app IsConfigured and database
        // context IsDatabaseConfigured - we still want to check CanConnect though to be safe
        internal void Startup()
        {
            UmbracoModule.EndRequest += UmbracoModule_EndRequest;
            UmbracoModule.RouteAttempt += UmbracoModule_RouteAttempt;

            if (DatabaseContext.CanConnect == false)
            {
                Logger.Warn<BatchedDatabaseServerMessenger>(
                    "Cannot connect to the database, distributed calls will not be enabled for this server.");
            }
            else
            {
                Boot();
            }
        }

        private void UmbracoModule_RouteAttempt(object sender, RoutableAttemptEventArgs e)
        {
            // as long as umbraco is ready & configured, sync
            switch (e.Outcome)
            {
                case EnsureRoutableOutcome.IsRoutable:
                case EnsureRoutableOutcome.NotDocumentRequest:
                case EnsureRoutableOutcome.NoContent:
                    Sync();
                    break;
                //case EnsureRoutableOutcome.NotReady:
                //case EnsureRoutableOutcome.NotConfigured:
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
            var idsA = ids?.ToArray();

            Type arrayType;
            if (GetArrayType(idsA, out arrayType) == false)
                throw new ArgumentException("All items must be of the same type, either int or Guid.", nameof(ids));

            BatchMessage(servers, refresher, messageType, idsA, arrayType, json);
        }

        public void FlushBatch()
        {
            var batch = GetBatch(false);
            if (batch == null) return;

            var instructions = batch.SelectMany(x => x.Instructions).ToArray();
            batch.Clear();

            //Write the instructions but only create JSON blobs with a max instruction count equal to MaxProcessingInstructionCount
            foreach (var instructionsBatch in instructions.InGroupsOf(Options.MaxProcessingInstructionCount))
            {
                WriteInstructions(instructionsBatch);
            }
            
        }

        private void WriteInstructions(IEnumerable<RefreshInstruction> instructions)
        {
            var dto = new CacheInstructionDto
            {
                UtcStamp = DateTime.UtcNow,
                Instructions = JsonConvert.SerializeObject(instructions, Formatting.None),
                OriginIdentity = LocalIdentity
            };

            Database.Insert(dto);
        }

        protected ICollection<RefreshInstructionEnvelope> GetBatch(bool create)
        {
            // try get the http context from the UmbracoContext, we do this because in the case we are launching an async
            // thread and we know that the cache refreshers will execute, we will ensure the UmbracoContext and therefore we
            // can get the http context from it
            var httpContext = (UmbracoContext.Current == null ? null : UmbracoContext.Current.HttpContext)
                // if this is null, it could be that an async thread is calling this method that we weren't aware of and the UmbracoContext
                // wasn't ensured at the beginning of the thread. We can try to see if the HttpContext.Current is available which might be 
                // the case if the asp.net synchronization context has kicked in
                ?? (HttpContext.Current == null ? null : new HttpContextWrapper(HttpContext.Current));

            // if no context was found, return null - we cannot not batch
            if (httpContext == null) return null;

            var key = typeof (BatchedDatabaseServerMessenger).Name;

            // no thread-safety here because it'll run in only 1 thread (request) at a time
            var batch = (ICollection<RefreshInstructionEnvelope>)httpContext.Items[key];
            if (batch == null && create)
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
            var instructions = RefreshInstruction.GetInstructions(refresher, messageType, ids, idType, json);

            // batch if we can, else write to DB immediately
            if (batch == null)
            {
                //only write the json blob with a maximum count of the MaxProcessingInstructionCount
                foreach (var maxBatch in instructions.InGroupsOf(Options.MaxProcessingInstructionCount))
                {
                    WriteInstructions(maxBatch);
                }
            }
            else
            {
                batch.Add(new RefreshInstructionEnvelope(servers, refresher, instructions));
            }
                
        }        
    }
}