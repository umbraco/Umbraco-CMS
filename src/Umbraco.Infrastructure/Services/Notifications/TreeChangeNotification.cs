using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public abstract class TreeChangeNotification<T> : EnumerableObjectNotification<T>
    {
        protected TreeChangeNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        protected TreeChangeNotification(IEnumerable<T> target, EventMessages messages) : base(target, messages)
        {
        }

        public IEnumerable<T> Changes => Target;
    }
}
