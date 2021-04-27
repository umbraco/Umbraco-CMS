using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class MediaTypeDeletingNotification : DeletingNotification<IMediaType>
    {
        public MediaTypeDeletingNotification(IMediaType target, EventMessages messages) : base(target, messages)
        {
        }

        public MediaTypeDeletingNotification(IEnumerable<IMediaType> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
