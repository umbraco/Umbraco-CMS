using System;
using System.Collections.Generic;
using System.ComponentModel;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Events
{
    [Obsolete("This is only used for the internal cache and will change, use tree change notifications instead")]
    [EditorBrowsable(EditorBrowsableState.Never)]
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
