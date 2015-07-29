using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// The status returned by many of the service methods
    /// </summary>
    public class OperationStatus<TEntity, TStatus> : OperationStatus<TStatus>
        where TStatus : struct
    {
        public OperationStatus(TEntity entity, TStatus statusType) : base(statusType)
        {
            Entity = entity;
        }

        public TEntity Entity { get; private set; }

    }

    public class OperationStatus<TStatus>
        where TStatus : struct
    {
        public OperationStatus(TStatus statusType)
        {
            StatusType = statusType;
        }

        public TStatus StatusType { get; internal set; }
    }

    /// <summary>
    /// The default operation status
    /// </summary>
    public class OperationStatus : OperationStatus<OperationStatusType>
    {
        public OperationStatus(OperationStatusType statusType) : base(statusType)
        {
        }


        #region Static Helper methods
        internal static OperationStatus Cancelled
        {
            get { return new OperationStatus(OperationStatusType.FailedCancelledByEvent); }
        }

        internal static OperationStatus Success
        {
            get { return new OperationStatus(OperationStatusType.Success); }
        } 
        #endregion
    }

}