using System;
using Umbraco.Core.Events;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// The status returned by many of the service methods
    /// </summary>
    public class OperationStatus<TEntity, TStatus> : OperationStatus<TStatus>
        where TStatus : struct
    {
        public OperationStatus(TEntity entity, TStatus statusType, EventMessages eventMessages) : base(statusType, eventMessages)
        {
            Entity = entity;
        }

        public TEntity Entity { get; private set; }

    }

    public class OperationStatus<TStatus>
        where TStatus : struct
    {
        public OperationStatus(TStatus statusType, EventMessages eventMessages)
        {
            if (eventMessages == null) throw new ArgumentNullException("eventMessages");
            StatusType = statusType;
            EventMessages = eventMessages;
        }

        public TStatus StatusType { get; internal set; }
        public EventMessages EventMessages { get; private set; }
    }

    /// <summary>
    /// The default operation status
    /// </summary>
    public class OperationStatus : OperationStatus<OperationStatusType>
    {
        public OperationStatus(OperationStatusType statusType, EventMessages eventMessages) : base(statusType, eventMessages)
        {
        }


        #region Static Helper methods

        internal static OperationStatus Cancelled(EventMessages eventMessages)
        {
            return new OperationStatus(OperationStatusType.FailedCancelledByEvent, eventMessages);
        }

        internal static OperationStatus Success(EventMessages eventMessages)
        {
            return new OperationStatus(OperationStatusType.Success, eventMessages);
        }

        #endregion
    }

}