using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Core.Publishing
{
    /// <summary>
    /// The result of unpublishing a content item
    /// </summary>
    public class UnPublishStatus : OperationStatus<IContent, UnPublishedStatusType>
    {
        public UnPublishStatus(IContent content, UnPublishedStatusType statusType, EventMessages eventMessages)
            : base(content, statusType, eventMessages)
        {
        }

        /// <summary>
        /// Creates a successful unpublish status
        /// </summary>
        public UnPublishStatus(IContent content, EventMessages eventMessages)
            : this(content, UnPublishedStatusType.Success, eventMessages)
        {
        }

        public IContent ContentItem
        {
            get { return Entity; }
        }        
    }
}