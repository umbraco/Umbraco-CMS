using System.Collections.Generic;

namespace Umbraco.Cms.Core.Events
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
