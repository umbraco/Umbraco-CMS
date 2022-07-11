using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Notifications;

public abstract class TreeChangeNotification<T> : EnumerableObjectNotification<TreeChange<T>>
{
    protected TreeChangeNotification(TreeChange<T> target, EventMessages messages)
        : base(target, messages)
    {
    }

    protected TreeChangeNotification(IEnumerable<TreeChange<T>> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public IEnumerable<TreeChange<T>> Changes => Target;
}
