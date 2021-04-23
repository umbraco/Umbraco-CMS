using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class ContentTypeSavingNotification : SavingNotification<IContentType>
    {
        public ContentTypeSavingNotification(IContentType target, EventMessages messages) : base(target, messages)
        {
        }

        public ContentTypeSavingNotification(IEnumerable<IContentType> target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
