using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Notifications;

public class ContentTreeChangeNotification : TreeChangeNotification<IContent>
{
    public ContentTreeChangeNotification(TreeChange<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public ContentTreeChangeNotification(IEnumerable<TreeChange<IContent>> target, EventMessages messages)
        : base(
        target, messages)
    {
    }

    public ContentTreeChangeNotification(
        IEnumerable<IContent> target,
        TreeChangeTypes changeTypes,
        EventMessages messages)
        : base(target.Select(x => new TreeChange<IContent>(x, changeTypes)), messages)
    {
    }

    public ContentTreeChangeNotification(
        IContent target,
        TreeChangeTypes changeTypes,
        EventMessages messages)
        : base(new TreeChange<IContent>(target, changeTypes), messages)
    {
    }
}
