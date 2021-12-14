using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Sync;
using Umbraco.Web.Routing;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Scoping;
using Umbraco.Web.Composing;
using System.ComponentModel;

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
        private readonly IUmbracoDatabaseFactory _databaseFactory;
        private readonly Lazy<SyncBootState> _syncBootState;

        [Obsolete("This overload should not be used, enableDistCalls has no effect")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public BatchedDatabaseServerMessenger(
            IRuntimeState runtime, IUmbracoDatabaseFactory databaseFactory, IScopeProvider scopeProvider, ISqlContext sqlContext, IProfilingLogger proflog, IGlobalSettings globalSettings, bool enableDistCalls, DatabaseServerMessengerOptions options)
            : this(runtime, databaseFactory, scopeProvider, sqlContext, proflog, globalSettings, options)
        { }

        public BatchedDatabaseServerMessenger(
            IRuntimeState runtime, IUmbracoDatabaseFactory databaseFactory, IScopeProvider scopeProvider, ISqlContext sqlContext, IProfilingLogger proflog, IGlobalSettings globalSettings, DatabaseServerMessengerOptions options)
            : base(runtime, scopeProvider, sqlContext, proflog, globalSettings, true, options)
        {
            _databaseFactory = databaseFactory;
            _syncBootState = new Lazy<SyncBootState>(() =>
            {
                if (_databaseFactory.CanConnect == false)
                {
                    Logger.Warn<BatchedDatabaseServerMessenger>("Cannot connect to the database, distributed calls will not be enabled for this server.");
                    return SyncBootState.Unknown;
                }
                else
                {
                    return base.GetSyncBootState();
                }
            });
        }

        // override to deal with database connectivity
        public override SyncBootState GetSyncBootState() => _syncBootState.Value;

        protected override void DeliverRemote(ICacheRefresher refresher, MessageType messageType, IEnumerable<object> ids = null, string json = null)
        {
            var idsA = ids?.ToArray();

            Type arrayType;
            if (GetArrayType(idsA, out arrayType) == false)
                throw new ArgumentException("All items must be of the same type, either int or Guid.", nameof(ids));

            BatchMessage(refresher, messageType, idsA, arrayType, json);
        }

        public void FlushBatch() => FlushBatch(null);

        internal void FlushBatch(HttpContextBase httpContext)
        {
            var batch = httpContext != null ? GetBatch(false, httpContext) : GetBatch(false);
            if (batch == null) return;

            var instructions = batch.SelectMany(x => x.Instructions).ToArray();
            batch.Clear();

            //Write the instructions but only create JSON blobs with a max instruction count equal to MaxProcessingInstructionCount
            using (var scope = ScopeProvider.CreateScope())
            {
                foreach (var instructionsBatch in instructions.InGroupsOf(Options.MaxProcessingInstructionCount))
                {
                    WriteInstructions(scope, instructionsBatch);
                }

                scope.Complete();
            }
        }

        private void WriteInstructions(IScope scope, IEnumerable<RefreshInstruction> instructions)
        {
            var dto = new CacheInstructionDto
            {
                UtcStamp = DateTime.UtcNow,
                Instructions = JsonConvert.SerializeObject(instructions, Formatting.None),
                OriginIdentity = LocalIdentity,
                InstructionCount = instructions.Sum(x => x.JsonIdCount)
            };
            scope.Database.Insert(dto);
        }

        protected ICollection<RefreshInstructionEnvelope> GetBatch(bool create)
        {
            // try get the http context from the UmbracoContext, we do this because in the case we are launching an async
            // thread and we know that the cache refreshers will execute, we will ensure the UmbracoContext and therefore we
            // can get the http context from it
            var httpContext = (Current.UmbracoContext == null ? null : Current.UmbracoContext.HttpContext)
                // if this is null, it could be that an async thread is calling this method that we weren't aware of and the UmbracoContext
                // wasn't ensured at the beginning of the thread. We can try to see if the HttpContext.Current is available which might be
                // the case if the asp.net synchronization context has kicked in
                ?? (HttpContext.Current == null ? null : new HttpContextWrapper(HttpContext.Current));

            // if no context was found, return null - we cannot batch
            if (httpContext == null) return null;

            return GetBatch(create, httpContext);
        }

        protected ICollection<RefreshInstructionEnvelope> GetBatch(bool create, HttpContextBase httpContext)
        {
            var key = typeof(BatchedDatabaseServerMessenger).Name;

            // no thread-safety here because it'll run in only 1 thread (request) at a time
            var batch = (ICollection<RefreshInstructionEnvelope>)httpContext.Items[key];
            if (batch == null && create)
                httpContext.Items[key] = batch = new List<RefreshInstructionEnvelope>();
            return batch;
        }

        protected void BatchMessage(
            ICacheRefresher refresher,
            MessageType messageType,
            IEnumerable<object> ids = null,
            Type idType = null,
            string json = null) => BatchMessage(refresher, messageType, null, ids, idType, json);

        protected void BatchMessage(
            ICacheRefresher refresher,
            MessageType messageType,
            HttpContextBase httpContext,
            IEnumerable<object> ids = null,
            Type idType = null,
            string json = null)
        {
            var batch = httpContext != null ? GetBatch(true, httpContext) : GetBatch(true);
            var instructions = RefreshInstruction.GetInstructions(refresher, messageType, ids, idType, json);

            // batch if we can, else write to DB immediately
            if (batch == null)
            {
                //only write the json blob with a maximum count of the MaxProcessingInstructionCount
                using (var scope = ScopeProvider.CreateScope())
                {
                    foreach (var maxBatch in instructions.InGroupsOf(Options.MaxProcessingInstructionCount))
                    {
                        WriteInstructions(scope, maxBatch);
                    }
                    scope.Complete();
                }
            }
            else
            {
                batch.Add(new RefreshInstructionEnvelope(refresher, instructions));
            }

        }
    }
}
