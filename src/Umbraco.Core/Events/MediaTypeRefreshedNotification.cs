using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Events
{
    public class MediaTypeRefreshedNotification : ContentTypeRefreshNotification<IMediaType>
    {
        public MediaTypeRefreshedNotification(ContentTypeChange<IMediaType> target, EventMessages messages) : base(target, messages)
        {
        }

        public MediaTypeRefreshedNotification(IEnumerable<ContentTypeChange<IMediaType>> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
