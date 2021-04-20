using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class MediaTypeDeletedNotification : DeletedNotification<IMediaType>
    {
        public MediaTypeDeletedNotification(IMediaType target, EventMessages messages) : base(target, messages)
        {
        }

        public MediaTypeDeletedNotification(IEnumerable<IMediaType> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
