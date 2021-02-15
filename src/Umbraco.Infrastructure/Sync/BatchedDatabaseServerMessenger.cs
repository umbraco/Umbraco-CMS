using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// An <see cref="IServerMessenger"/> implementation that works by storing messages in the database.
    /// </summary>
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
            IServerRoleAccessor serverRegistrar,
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

        /// <inheritdoc/>
        protected override void DeliverRemote(ICacheRefresher refresher, MessageType messageType, IEnumerable<object> ids = null, string json = null)
        {
            var idsA = ids?.ToArray();

            if (GetArrayType(idsA, out Type arrayType) == false)
            {
                throw new ArgumentException("All items must be of the same type, either int or Guid.", nameof(ids));
            }

            BatchMessage(refresher, messageType, idsA, arrayType, json);
        }

        /// <inheritdoc/>
        public override void SendMessages()
        {
            ICollection<RefreshInstructionEnvelope> batch = GetBatch(false);
            if (batch == null)
            {
                return;
            }

            RefreshInstruction[] instructions = batch.SelectMany(x => x.Instructions).ToArray();
            batch.Clear();

            // Write the instructions but only create JSON blobs with a max instruction count equal to MaxProcessingInstructionCount
            using (IScope scope = ScopeProvider.CreateScope())
            {
                foreach (IEnumerable<RefreshInstruction> instructionsBatch in instructions.InGroupsOf(GlobalSettings.DatabaseServerMessenger.MaxProcessingInstructionCount))
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

        private ICollection<RefreshInstructionEnvelope> GetBatch(bool create)
        {
            var key = nameof(BatchedDatabaseServerMessenger);

            if (!_requestCache.IsAvailable)
            {
                return null;
            }

            // no thread-safety here because it'll run in only 1 thread (request) at a time
            var batch = (ICollection<RefreshInstructionEnvelope>)_requestCache.Get(key);
            if (batch == null && create)
            {
                batch = new List<RefreshInstructionEnvelope>();
                _requestCache.Set(key, batch);
            }

            return batch;
        }

        private void BatchMessage(
            ICacheRefresher refresher,
            MessageType messageType,
            IEnumerable<object> ids = null,
            Type idType = null,
            string json = null)
        {
            ICollection<RefreshInstructionEnvelope> batch = GetBatch(true);
            IEnumerable<RefreshInstruction> instructions = RefreshInstruction.GetInstructions(refresher, messageType, ids, idType, json);

            // batch if we can, else write to DB immediately
            if (batch == null)
            {
                // only write the json blob with a maximum count of the MaxProcessingInstructionCount
                using (IScope scope = ScopeProvider.CreateScope())
                {
                    foreach (IEnumerable<RefreshInstruction> maxBatch in instructions.InGroupsOf(GlobalSettings.DatabaseServerMessenger.MaxProcessingInstructionCount))
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
