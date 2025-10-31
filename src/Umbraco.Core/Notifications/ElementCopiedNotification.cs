using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// The notification is published after the element has been copied.
/// </summary>
public sealed class ElementCopiedNotification : CopiedNotification<IElement>
{
    public ElementCopiedNotification(IElement original, IElement copy, int parentId, Guid? parentKey, bool relateToOriginal, EventMessages messages)
        : base(original, copy, parentId, parentKey, relateToOriginal, messages)
    {
    }
}
