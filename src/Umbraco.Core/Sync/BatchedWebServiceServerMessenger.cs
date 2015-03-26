using System;
using System.Collections.Generic;
using System.Linq;
using umbraco.interfaces;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// An <see cref="IServerMessenger"/> that works by messaging servers via web services.
    /// </summary>
    /// <remarks>
    /// Abstract because it needs to be inherited by a class that will
    /// - implement ProcessBatch()
    /// - trigger FlushBatch() when appropriate
    /// </remarks>
    internal abstract class BatchedWebServiceServerMessenger : WebServiceServerMessenger
    {
        internal BatchedWebServiceServerMessenger()
        {
        }

        internal BatchedWebServiceServerMessenger(string login, string password)
            : base(login, password)
        {
        }

        internal BatchedWebServiceServerMessenger(string login, string password, bool useDistributedCalls)
            : base(login, password, useDistributedCalls)
        {
        }

        protected BatchedWebServiceServerMessenger(Func<Tuple<string, string>> getLoginAndPassword)
            : base(getLoginAndPassword)
        {
        }

        protected abstract ICollection<RefreshInstructionEnvelope> GetBatch(bool ensureHttpContext);

        protected void FlushBatch()
        {
            var batch = GetBatch(false);
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
            var batch = GetBatch(true);
            if (batch == null)
                throw new Exception("Failed to get a batch.");

            batch.Add(new RefreshInstructionEnvelope(servers, refresher,
                RefreshInstruction.GetInstructions(refresher, messageType, ids, idType, json)));
        }
    }
}
