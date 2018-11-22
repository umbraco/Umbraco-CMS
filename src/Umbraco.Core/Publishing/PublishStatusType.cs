namespace Umbraco.Core.Publishing
{
    /// <summary>
    /// A status type of the result of publishing a content item
    /// </summary>
    /// <remarks>
    /// Anything less than 10 = Success!
    /// </remarks>
    public enum PublishStatusType
    {
        /// <summary>
        /// The publishing was successful.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The item was already published
        /// </summary>
        SuccessAlreadyPublished = 1,

        /// <summary>
        /// The content could not be published because it's ancestor path isn't published
        /// </summary>
        FailedPathNotPublished = 10,

        /// <summary>
        /// The content item was scheduled to be un-published and it has expired so we cannot force it to be
        /// published again as part of a bulk publish operation.
        /// </summary>
        FailedHasExpired = 11,

        /// <summary>
        /// The content item is scheduled to be released in the future and therefore we cannot force it to 
        /// be published during a bulk publish operation.
        /// </summary>
        FailedAwaitingRelease = 12,

        /// <summary>
        /// The content item is in the trash, it cannot be published
        /// </summary>
        FailedIsTrashed = 13,

        /// <summary>
        /// The publish action has been cancelled by an event handler
        /// </summary>
        FailedCancelledByEvent = 14,

        /// <summary>
        /// The content item contains invalid data (has not passed validation requirements)
        /// </summary>
        FailedContentInvalid = 15
    }
}