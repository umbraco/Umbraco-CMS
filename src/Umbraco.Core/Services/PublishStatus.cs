using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the result of publishing a content item.
    /// </summary>
    public class PublishStatus : OperationStatus<PublishStatusType, IContent>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PublishStatus"/> class with a status type, event messages, and a content item.
        /// </summary>
        /// <param name="statusType">The status of the operation.</param>
        /// <param name="eventMessages">Event messages produced by the operation.</param>
        /// <param name="content">The content item.</param>
        public PublishStatus(PublishStatusType statusType, EventMessages eventMessages, IContent content)
            : base(statusType, eventMessages, content)
        { }

        /// <summary>
        /// Creates a new successful instance of the <see cref="PublishStatus"/> class with a event messages, and a content item.
        /// </summary>
        /// <param name="eventMessages">Event messages produced by the operation.</param>
        /// <param name="content">The content item.</param>
        public PublishStatus(IContent content, EventMessages eventMessages)
            : base(PublishStatusType.Success, eventMessages, content)
        { }

        /// <summary>
        /// Gets the content item.
        /// </summary>
        public IContent ContentItem => Value;

        /// <summary>
        /// Gets or sets the invalid properties, if the status failed due to validation.
        /// </summary>
        public IEnumerable<Property> InvalidProperties { get; set; }
    }
}