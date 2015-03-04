using System;
using System.Collections.Generic;
using System.Linq;
using umbraco.interfaces;

namespace Umbraco.Core.Sync
{
    // abstract because it needs to be inherited by a class that will
    // - implement ProcessBatch()
    // - trigger FlushBatch() when appropriate
    //
    internal abstract class BatchedWebServiceServerMessenger : WebServiceServerMessenger
    {
        private readonly Func<bool, ICollection<RefreshInstructionEnvelope>> _getBatch;

        internal BatchedWebServiceServerMessenger(Func<bool, ICollection<RefreshInstructionEnvelope>> getBatch)
        {
            if (getBatch == null)
                throw new ArgumentNullException("getBatch");

            _getBatch = getBatch;
        }

        internal BatchedWebServiceServerMessenger(string login, string password, Func<bool, ICollection<RefreshInstructionEnvelope>> getBatch)
            : base(login, password)
        {
            if (getBatch == null)
                throw new ArgumentNullException("getBatch");

            _getBatch = getBatch;
        }

        internal BatchedWebServiceServerMessenger(string login, string password, bool useDistributedCalls, Func<bool, ICollection<RefreshInstructionEnvelope>> getBatch)
            : base(login, password, useDistributedCalls)
        {
            if (getBatch == null)
                throw new ArgumentNullException("getBatch");

            _getBatch = getBatch;
        }

        protected BatchedWebServiceServerMessenger(Func<Tuple<string, string>> getLoginAndPassword, Func<bool, ICollection<RefreshInstructionEnvelope>> getBatch)
            : base(getLoginAndPassword)
        {
            if (getBatch == null)
                throw new ArgumentNullException("getBatch");

            _getBatch = getBatch;
        }

        protected void FlushBatch()
        {
            var batch = _getBatch(false);
            if (batch == null) return;

            var batcha = batch.ToArray();
            batch.Clear();
            if (batcha.Length == 0) return;

            ProcessBatch(batcha);
        }

        // needs to be overriden to actually do something
        protected abstract void ProcessBatch(RefreshInstructionEnvelope[] batch);

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
