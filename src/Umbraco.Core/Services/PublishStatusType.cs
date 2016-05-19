namespace Umbraco.Core.Services
{
    /// <summary>
    /// A value indicating the result of publishing a content item.
    /// </summary>
    /// <remarks>Do NOT compare against a hard-coded numeric value to check for success or failure,
    /// but instead use the IsSuccess() extension method defined below - which should be the unique
    /// place where the numeric test should take place.
    /// </remarks>
    public enum PublishStatusType
    {
        /// <summary>
        /// The publishing was successful.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The item was already published.
        /// </summary>
        SuccessAlreadyPublished = 1,

        // Values below this value indicate a success, values above it indicate a failure.
        // This value is considered a failure.
        //Reserved = 10,

        /// <summary>
        /// The content could not be published because it's ancestor path isn't published.
        /// </summary>
        FailedPathNotPublished = 11,

        /// <summary>
        /// The content item was scheduled to be un-published and it has expired so we cannot force it to be
        /// published again as part of a bulk publish operation.
        /// </summary>
        FailedHasExpired = 12,

        /// <summary>
        /// The content item is scheduled to be released in the future and therefore we cannot force it to 
        /// be published during a bulk publish operation.
        /// </summary>
        FailedAwaitingRelease = 13,

        /// <summary>
        /// The content item could not be published because it is in the trash.
        /// </summary>
        FailedIsTrashed = 14,

        /// <summary>
        /// The publish action has been cancelled by an event handler.
        /// </summary>
        FailedCancelledByEvent = 15,

        /// <summary>
        /// The content item could not be published because it contains invalid data (has not passed validation requirements).
        /// </summary>
        FailedContentInvalid = 16
    }

    /// <summary>
    /// Provides extension methods for the <see cref="PublishStatusType"/> enum.
    /// </summary>
    public static class PublicStatusTypeExtensions
    {
        /// <summary>
        /// Gets a value indicating whether the status indicates a success.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns>A value indicating whether the status indicates a success.</returns>
        public static bool IsSuccess(this PublishStatusType status)
        {
            return (int) status < 10;
        }

        /// <summary>
        /// Gets a value indicating whether the status indicates a failure.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns>A value indicating whether the status indicates a failure.</returns>
        public static bool IsFailure(this PublishStatusType status)
        {
            return (int) status >= 10;
        }
    }
}