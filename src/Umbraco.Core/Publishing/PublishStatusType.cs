namespace Umbraco.Core.Publishing
{
    /// <summary>
    /// A status type of the result of publishing a content item
    /// </summary>
    public enum PublishStatusType
    {
        /// <summary>
        /// The publishing was successful.
        /// </summary>
        Success,
        
        /// <summary>
        /// The content item was scheduled to be un-published and it has expired so we cannot force it to be
        /// published again as part of a bulk publish operation.
        /// </summary>
        FailedHasExpired,

        /// <summary>
        /// The content item is scheduled to be released in the future and therefore we cannot force it to 
        /// be published during a bulk publish operation.
        /// </summary>
        FailedAwaitingRelease,

        /// <summary>
        /// The content item is in the trash, it cannot be published
        /// </summary>
        FailedIsTrashed,

        /// <summary>
        /// The publish action has been cancelled by an event handler
        /// </summary>
        FailedCancelledByEvent,

        /// <summary>
        /// The content item contains invalid data (has not passed validation requirements)
        /// </summary>
        FailedContentInvalid
    }
}