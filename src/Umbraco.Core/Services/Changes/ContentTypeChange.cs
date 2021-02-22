using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Changes
{
    public class ContentTypeChange<TItem>
        where TItem : class, IContentTypeComposition
    {
        public ContentTypeChange(TItem item, ContentTypeChangeTypes changeTypes)
        {
            Item = item;
            ChangeTypes = changeTypes;
        }

        public TItem Item { get; }

        public ContentTypeChangeTypes ChangeTypes { get; set; }

        public EventArgs ToEventArgs(ContentTypeChange<TItem> change)
        {
            return new EventArgs(change);
        }

        public class EventArgs : System.EventArgs
        {
            public EventArgs(IEnumerable<ContentTypeChange<TItem>> changes)
            {
                Changes = changes.ToArray();
            }

            public EventArgs(ContentTypeChange<TItem> change)
                : this(new[] { change })
            { }

            public IEnumerable<ContentTypeChange<TItem>> Changes { get; private set; }
        }
    }

}
