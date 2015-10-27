namespace Umbraco.Core.Services
{
    /// <summary>
    /// A status type of the result of saving an item
    /// </summary>
    /// <remarks>
    /// Anything less than 10 = Success!
    /// </remarks>
    public enum OperationStatusType
    {
        /// <summary>
        /// The saving was successful.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The saving has been cancelled by a 3rd party add-in
        /// </summary>
        FailedCancelledByEvent = 14

        //TODO: In the future, we might need to add more operations statuses, potentially like 'FailedByPermissions', etc...
    }
}