using Umbraco.Core.Events;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the result of unpublishing a content item.
    /// </summary>
    public class UnPublishStatus : OperationStatus<UnPublishedStatusType, IContent>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="UnPublishStatus"/> class with a status type, event messages, and a content item.
        /// </summary>
        /// <param name="statusType">The status of the operation.</param>
        /// <param name="eventMessages">Event messages produced by the operation.</param>
        /// <param name="content">The content item.</param>
        public UnPublishStatus(UnPublishedStatusType statusType, EventMessages eventMessages, IContent content)
            : base(statusType, eventMessages, content)
        { }

        /// <summary>
        /// Creates a new successful instance of the <see cref="UnPublishStatus"/> class with a event messages, and a content item.
        /// </summary>
        /// <param name="eventMessages">Event messages produced by the operation.</param>
        /// <param name="content">The content item.</param>
        public UnPublishStatus(IContent content, EventMessages eventMessages)
            : base(UnPublishedStatusType.Success, eventMessages, content)
        { }

        /// <summary>
        /// Gets the content item.
        /// </summary>
        public IContent ContentItem => Value;
    }
}