using System;
using System.ComponentModel;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    [Obsolete("This is only used for the internal cache and will change, use saved notifications instead")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ContentRefreshNotification : EntityRefreshNotification<IContent>
    {
        public ContentRefreshNotification(IContent target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
