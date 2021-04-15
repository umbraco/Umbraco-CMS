using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Events
{
    public class ContentTypeRefreshedNotification : ContentTypeRefreshNotification<IContentType>
    {
        public ContentTypeRefreshedNotification(ContentTypeChange<IContentType> target, EventMessages messages) : base(target, messages)
        {
        }

        public ContentTypeRefreshedNotification(IEnumerable<ContentTypeChange<IContentType>> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
