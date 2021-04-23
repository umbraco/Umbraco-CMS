using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Events
{
    public abstract class ContentTypeChangeNotification<T> : EnumerableObjectNotification<ContentTypeChange<T>> where T : class, IContentTypeComposition
    {
        protected ContentTypeChangeNotification(ContentTypeChange<T> target, EventMessages messages) : base(target, messages)
        {
        }

        protected ContentTypeChangeNotification(IEnumerable<ContentTypeChange<T>> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<ContentTypeChange<T>> Changes => Target;
    }
}
