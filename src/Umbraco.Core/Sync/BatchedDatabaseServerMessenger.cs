using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core.Models.Rdbms;
using umbraco.interfaces;

namespace Umbraco.Core.Sync
{
    // abstract because it needs to be inherited by a class that will
    // - trigger FlushBatch() when appropriate
    // - trigger Boot() when appropriate
    // - trigger Sync() when appropriate
    //
    public abstract class BatchedDatabaseServerMessenger : DatabaseServerMessenger
    {
        private readonly Func<bool, ICollection<RefreshInstructionEnvelope>> _getBatch;

        protected BatchedDatabaseServerMessenger(ApplicationContext appContext, bool enableDistCalls, DatabaseServerMessengerOptions options,
            Func<bool, ICollection<RefreshInstructionEnvelope>> getBatch)
            : base(appContext, enableDistCalls, options)
        {
            if (getBatch == null)
                throw new ArgumentNullException("getBatch");

            _getBatch = getBatch;
        }

        public void FlushBatch()
        {
            var batch = _getBatch(false);
            if (batch == null) return;

            var instructions = batch.SelectMany(x => x.Instructions).ToArray();
            batch.Clear();
            if (instructions.Length == 0) return;

            var dto = new CacheInstructionDto
            {
                UtcStamp = DateTime.UtcNow,
                Instructions = JsonConvert.SerializeObject(instructions, Formatting.None),
                OriginIdentity = GetLocalIdentity()
            };

            ApplicationContext.DatabaseContext.Database.Insert(dto);
        }

        protected override void DeliverRemote(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, MessageType messageType, IEnumerable<object> ids = null, string json = null)
        {
            var idsA = ids == null ? null : ids.ToArray();

            Type arrayType;
            if (GetArrayType(idsA, out arrayType) == false)
                throw new ArgumentException("All items must be of the same type, either int or Guid.", "ids");

            BatchMessage(servers, refresher, messageType, idsA, arrayType, json);
        }

        protected void BatchMessage(
            IEnumerable<IServerAddress> servers,
            ICacheRefresher refresher,
            MessageType messageType,
            IEnumerable<object> ids = null,
            Type idType = null,
            string json = null)
        {
            var batch = _getBatch(true);
            if (batch == null)
                throw new Exception("Failed to get a batch.");

            batch.Add(new RefreshInstructionEnvelope(servers, refresher,
                RefreshInstruction.GetInstructions(refresher, messageType, ids, idType, json)));
        }
    }
}
