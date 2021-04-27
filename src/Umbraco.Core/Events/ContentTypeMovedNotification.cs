using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class ContentTypeMovedNotification : MovedNotification<IContentType>
    {
        public ContentTypeMovedNotification(MoveEventInfo<IContentType> target, EventMessages messages) : base(target, messages)
        {
        }

        public ContentTypeMovedNotification(IEnumerable<MoveEventInfo<IContentType>> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
