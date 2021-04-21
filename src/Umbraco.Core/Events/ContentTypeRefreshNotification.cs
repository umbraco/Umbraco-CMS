using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Events
{
    public abstract class ContentTypeRefreshNotification<T> : ContentTypeChangeNotification<T> where T: class, IContentTypeComposition
    {
        protected ContentTypeRefreshNotification(ContentTypeChange<T> target, EventMessages messages) : base(target, messages)
        {
        }

        protected ContentTypeRefreshNotification(IEnumerable<ContentTypeChange<T>> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
