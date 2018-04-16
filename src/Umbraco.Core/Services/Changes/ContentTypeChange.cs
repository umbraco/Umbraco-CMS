using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services.Changes
{
    internal class ContentTypeChange<TItem>
        where TItem : class, IContentTypeComposition
    {
        public ContentTypeChange(TItem item, ContentTypeChangeTypes changeTypes)
        {
            Item = item;
            ChangeTypes = changeTypes;
        }

        public TItem Item { get; }

        public ContentTypeChangeTypes ChangeTypes { get; internal set; }

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
