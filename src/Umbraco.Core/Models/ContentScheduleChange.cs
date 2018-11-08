namespace Umbraco.Core.Models
{
    // fixme/review - should this be named DocumentScheduleAction?
    // fixme/review - should values be Release and Expire not Start and Stop?

    /// <summary>
    /// Defines scheduled actions for documents.
    /// </summary>
    public enum ContentScheduleChange
    {
        /// <summary>
        /// Release the document.
        /// </summary>
        Start,

        /// <summary>
        /// Expire the document.
        /// </summary>
        End
    }
}
