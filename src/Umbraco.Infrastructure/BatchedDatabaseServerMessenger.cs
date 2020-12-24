using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Scoping;
using Umbraco.Core.Sync;

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
        private readonly IRequestCache _requestCache;
        private readonly IRequestAccessor _requestAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchedDatabaseServerMessenger"/> class.
        /// </summary>
        public BatchedDatabaseServerMessenger(
            IMainDom mainDom,
            IScopeProvider scopeProvider,
            IProfilingLogger proflog,
            ILogger<BatchedDatabaseServerMessenger> logger,
            IServerRegistrar serverRegistrar,
            DatabaseServerMessengerCallbacks callbacks,
            IHostingEnvironment hostingEnvironment,
            CacheRefresherCollection cacheRefreshers,
            IRequestCache requestCache,
            IRequestAccessor requestAccessor,
            IOptions<GlobalSettings> globalSettings)
            : base(mainDom, scopeProvider, proflog, logger, serverRegistrar, true, callbacks, hostingEnvironment, cacheRefreshers, globalSettings)
        {
            _requestCache = requestCache;
            _requestAccessor = requestAccessor;
        }

        protected override void DeliverRemote(ICacheRefresher refresher, MessageType messageType, IEnumerable<object> ids = null, string json = null)
        {
            var idsA = ids?.ToArray();

            Type arrayType;
            if (GetArrayType(idsA, out arrayType) == false)
                throw new ArgumentException("All items must be of the same type, either int or Guid.", nameof(ids));

            BatchMessage(refresher, messageType, idsA, arrayType, json);
        }

        public override void SendMessages()
        {
            var batch = GetBatch(false);
            if (batch == null) return;

            var instructions = batch.SelectMany(x => x.Instructions).ToArray();
            batch.Clear();

            // Write the instructions but only create JSON blobs with a max instruction count equal to MaxProcessingInstructionCount
            using (var scope = ScopeProvider.CreateScope())
            {
                foreach (var instructionsBatch in instructions.InGroupsOf(GlobalSettings.DatabaseServerMessenger.MaxProcessingInstructionCount))
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
            var key = nameof(BatchedDatabaseServerMessenger);

            if (!_requestCache.IsAvailable) return null;

            // no thread-safety here because it'll run in only 1 thread (request) at a time
            var batch = (ICollection<RefreshInstructionEnvelope>)_requestCache.Get(key);
            if (batch == null && create)
            {
                batch = new List<RefreshInstructionEnvelope>();
                _requestCache.Set(key, batch);
            }

            return batch;
        }

        protected void BatchMessage(
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
                using (var scope = ScopeProvider.CreateScope())
                {
                    foreach (var maxBatch in instructions.InGroupsOf(GlobalSettings.DatabaseServerMessenger.MaxProcessingInstructionCount))
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
