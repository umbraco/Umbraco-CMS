using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Core.Publishing
{
    /// <summary>
    /// The result of publishing a content item
    /// </summary>
    public class PublishStatus : OperationStatus<IContent, PublishStatusType>
    {
        public PublishStatus(IContent content, PublishStatusType statusType)
            : base(content, statusType)
        {            
        }

        /// <summary>
        /// Creates a successful publish status
        /// </summary>
        public PublishStatus(IContent content)
            : this(content, PublishStatusType.Success)
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