using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class MediaTypeSavedNotification : SavedNotification<IMediaType>
    {
        public MediaTypeSavedNotification(IMediaType target, EventMessages messages) : base(target, messages)
        {
        }

        public MediaTypeSavedNotification(IEnumerable<IMediaType> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
