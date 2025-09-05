using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Notifications;

public sealed class ElementTreeChangeNotification : TreeChangeNotification<IElement>
{
    public ElementTreeChangeNotification(TreeChange<IElement> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ElementTreeChangeNotification(IEnumerable<TreeChange<IElement>> target, EventMessages messages)
        : base(
            target, messages)
    {
    }

    public ElementTreeChangeNotification(
        IEnumerable<IElement> target,
        TreeChangeTypes changeTypes,
        EventMessages messages)
        : base(target.Select(x => new TreeChange<IElement>(x, changeTypes)), messages)
    {
    }

    public ElementTreeChangeNotification(
        IElement target,
        TreeChangeTypes changeTypes,
        EventMessages messages)
        : base(new TreeChange<IElement>(target, changeTypes), messages)
    {
    }

    public ElementTreeChangeNotification(
        IElement target,
        TreeChangeTypes changeTypes,
        IEnumerable<string>? publishedCultures,
        IEnumerable<string>? unpublishedCultures,
        EventMessages messages)
        : base(new TreeChange<IElement>(target, changeTypes, publishedCultures, unpublishedCultures), messages)
    {
    }
}
