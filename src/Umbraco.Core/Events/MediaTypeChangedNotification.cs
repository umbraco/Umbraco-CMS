using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Events
{
    public class MediaTypeChangedNotification : ContentTypeChangeNotification<IMediaType>
    {
        public MediaTypeChangedNotification(ContentTypeChange<IMediaType> target, EventMessages messages) : base(target, messages)
        {
        }

        public MediaTypeChangedNotification(IEnumerable<ContentTypeChange<IMediaType>> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
