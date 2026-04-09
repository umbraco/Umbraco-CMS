using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// The notification is published after a copy object has been created and had its parentId updated.
/// </summary>
public sealed class ElementCopyingNotification : CopyingNotification<IElement>
{
    public ElementCopyingNotification(IElement original, IElement copy, int parentId, Guid? parentKey, EventMessages messages)
        : base(original, copy, parentId, parentKey, messages)
    {
    }
}
