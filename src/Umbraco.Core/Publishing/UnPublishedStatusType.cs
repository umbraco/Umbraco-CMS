namespace Umbraco.Core.Publishing
{
    /// <summary>
    /// A status type of the result of unpublishing a content item
    /// </summary>
    /// <remarks>
    /// Anything less than 10 = Success!
    /// </remarks>
    public enum UnPublishedStatusType
    {
        /// <summary>
        /// The unpublishing was successful.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The item was already unpublished
        /// </summary>
        SuccessAlreadyUnPublished = 1,

        /// <summary>
        /// The publish action has been cancelled by an event handler
        /// </summary>
        FailedCancelledByEvent = 14,
    }
}