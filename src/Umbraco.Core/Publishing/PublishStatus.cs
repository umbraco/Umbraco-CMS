using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Core.Publishing
{
    /// <summary>
    /// The result of publishing a content item
    /// </summary>
    public class PublishStatus : OperationStatus<IContent, PublishStatusType>
    {
        public PublishStatus(IContent content, PublishStatusType statusType, EventMessages eventMessages)
            : base(content, statusType, eventMessages)
        {            
        }

        /// <summary>
        /// Creates a successful publish status
        /// </summary>
        public PublishStatus(IContent content, EventMessages eventMessages)
            : this(content, PublishStatusType.Success, eventMessages)
        {
        }

        public IContent ContentItem
        {
            get { return Entity; }
        }

        /// <summary>
        /// Gets sets the invalid properties if the status failed due to validation.
        /// </summary>
        public IEnumerable<Property> InvalidProperties { get; set; }
    }
}