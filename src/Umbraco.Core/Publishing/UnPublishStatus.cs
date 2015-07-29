using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Core.Publishing
{
    /// <summary>
    /// The result of unpublishing a content item
    /// </summary>
    public class UnPublishStatus : OperationStatus<IContent, UnPublishedStatusType>
    {
        public UnPublishStatus(IContent content, UnPublishedStatusType statusType)
            : base(content, statusType)
        {
        }

        /// <summary>
        /// Creates a successful unpublish status
        /// </summary>
        public UnPublishStatus(IContent content)
            : this(content, UnPublishedStatusType.Success)
        {
        }

        public IContent ContentItem
        {
            get { return Entity; }
        }        
    }
}