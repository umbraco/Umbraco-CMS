using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class MediaTypeMovedNotification : MovedNotification<IMediaType>
    {
        public MediaTypeMovedNotification(MoveEventInfo<IMediaType> target, EventMessages messages) : base(target, messages)
        {
        }

        public MediaTypeMovedNotification(IEnumerable<MoveEventInfo<IMediaType>> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
