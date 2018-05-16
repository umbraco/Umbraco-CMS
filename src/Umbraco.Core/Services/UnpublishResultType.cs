namespace Umbraco.Core.Services
{
    /// <summary>
    /// A value indicating the result of unpublishing a content item.
    /// </summary>
    public enum UnpublishResultType : byte
    {
        /// <summary>
        /// The unpublishing was successful.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The item was already unpublished.
        /// </summary>
        SuccessAlready = 1,

        /// <summary>
        /// The specified variant was unpublished, the content item itself remains published.
        /// </summary>
        SuccessVariant = 2,

        /// <summary>
        /// The specified variant was a mandatory culture therefore it was unpublished and the content item itself is unpublished
        /// </summary>
        SuccessMandatoryCulture = 3,

        /// <summary>
        /// The operation failed.
        /// </summary>
        /// <remarks>All values above this value indicate a failure.</remarks>
        Failed = 128,

        /// <summary>
        /// The publish action has been cancelled by an event handler.
        /// </summary>
        FailedCancelledByEvent = Failed | 5,
    }
}
