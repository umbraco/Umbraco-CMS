using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class ContentTypeSavedNotification : SavedNotification<IContentType>
    {
        public ContentTypeSavedNotification(IContentType target, EventMessages messages) : base(target, messages)
        {
        }

        public ContentTypeSavedNotification(IEnumerable<IContentType> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
