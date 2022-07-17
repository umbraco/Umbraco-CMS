using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class EntityRefreshNotification<T> : ObjectNotification<T>
    where T : class, IContentBase
{
    public EntityRefreshNotification(T target, EventMessages messages)
        : base(target, messages)
    {
    }

    public T Entity => Target;
}
