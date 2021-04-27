using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class ContentTypeDeletingNotification : DeletingNotification<IContentType>
    {
        public ContentTypeDeletingNotification(IContentType target, EventMessages messages) : base(target, messages)
        {
        }

        public ContentTypeDeletingNotification(IEnumerable<IContentType> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
