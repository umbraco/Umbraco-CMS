using Umbraco.Core.Models;

namespace Umbraco.Core.Publishing
{
    /// <summary>
    /// The result of publishing a content item
    /// </summary>
    public class PublishStatus
    {
        public IContent ContentItem { get; private set; }
        public PublishStatusType StatusType { get; private set; }

        public PublishStatus(IContent content, PublishStatusType statusType)
        {
            ContentItem = content;
            StatusType = statusType;
        }

        /// <summary>
        /// Creates a successful publish status
        /// </summary>
        public PublishStatus(IContent content)
            : this(content, PublishStatusType.Success)
        {            
        }

        
    }
}