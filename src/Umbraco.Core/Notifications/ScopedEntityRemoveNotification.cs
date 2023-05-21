using System.ComponentModel;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

[Obsolete("This is only used for the internal cache and will change, use tree change notifications instead")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class ScopedEntityRemoveNotification : ObjectNotification<IContentBase>
{
    public ScopedEntityRemoveNotification(IContentBase target, EventMessages messages)
        : base(target, messages)
    {
    }

    public IContentBase Entity => Target;
}
