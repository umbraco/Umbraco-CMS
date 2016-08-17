namespace Umbraco.Core.Services
{
    /// <summary>
    /// A value indicating the result of an operation.
    /// </summary>
    /// <remarks>Do NOT compare against a hard-coded numeric value to check for success or failure,
    /// but instead use the IsSuccess() extension method defined below - which should be the unique
    /// place where the numeric test should take place.
    /// </remarks>
    public enum OperationStatusType
    {
        /// <summary>
        /// The operation was successful.
        /// </summary>
        Success = 0,

        // Values below this value indicate a success, values above it indicate a failure.
        // This value is considered a failure.
        //Reserved = 10,

        /// <summary>
        /// The operation could not complete because of invalid preconditions (eg creating a reference
        /// to an item that does not exist).
        /// </summary>
        FailedCannot = 12,

        /// <summary>
        /// The operation has been cancelled by an event handler.
        /// </summary>
        FailedCancelledByEvent = 14,

        /// <summary>
        /// The operation could not complete due to an exception.
        /// </summary>
        FailedExceptionThrown = 15,

        /// <summary>
        /// No operation has been executed because it was not needed (eg deleting an item that doesn't exist).
        /// </summary>
        NoOperation = 100,

        //TODO: In the future, we might need to add more operations statuses, potentially like 'FailedByPermissions', etc...
    }

    /// <summary>
    /// Provides extension methods for the <see cref="OperationStatusType"/> enum.
    /// </summary>
    public static class OperationStatusTypeExtensions
    {
        /// <summary>
        /// Gets a value indicating whether the status indicates a success.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns>A value indicating whether the status indicates a success.</returns>
        public static bool IsSuccess(this OperationStatusType status)
        {
            return (int) status < 10;
        }

        /// <summary>
        /// Gets a value indicating whether the status indicates a failure.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns>A value indicating whether the status indicates a failure.</returns>
        public static bool IsFailure(this OperationStatusType status)
        {
            return (int) status >= 10;
        }
    }
}