namespace Umbraco.Core.Services
{
    /// <summary>
    /// The status returned by many of the service methods
    /// </summary>
    public class OperationStatus
    {
        //TODO: This is a pretty simple class atm, but is 'future' proofed in case we need to add more detail here

        internal static OperationStatus Cancelled
        {
            get { return new OperationStatus(OperationStatusType.FailedCancelledByEvent);}
        }

        internal static OperationStatus Success
        {
            get { return new OperationStatus(OperationStatusType.Success); }
        }

        public OperationStatus(OperationStatusType statusType)         
        {
            StatusType = statusType;
        }
        public OperationStatusType StatusType { get; internal set; }
    }
}