using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Notifications;

public class MediaTreeChangeNotification : TreeChangeNotification<IMedia>
{
    public MediaTreeChangeNotification(TreeChange<IMedia> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MediaTreeChangeNotification(IEnumerable<TreeChange<IMedia>> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public MediaTreeChangeNotification(
        IEnumerable<IMedia> target,
        TreeChangeTypes changeTypes,
        EventMessages messages)
        : base(target.Select(x => new TreeChange<IMedia>(x, changeTypes)), messages)
    {
    }

    public MediaTreeChangeNotification(IMedia target, TreeChangeTypes changeTypes, EventMessages messages)
        : base(new TreeChange<IMedia>(target, changeTypes), messages)
    {
    }
}
