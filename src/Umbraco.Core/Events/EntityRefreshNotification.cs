using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class EntityRefreshNotification<T> : ObjectNotification<T> where T : class, IContentBase
    {
        public EntityRefreshNotification(T target, EventMessages messages) : base(target, messages)
        {
        }

        public T Entity => Target;
    }
}
