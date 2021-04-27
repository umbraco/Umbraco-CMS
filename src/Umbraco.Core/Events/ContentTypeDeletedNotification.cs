using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class ContentTypeDeletedNotification : DeletedNotification<IContentType>
    {
        public ContentTypeDeletedNotification(IContentType target, EventMessages messages) : base(target, messages)
        {
        }

        public ContentTypeDeletedNotification(IEnumerable<IContentType> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
