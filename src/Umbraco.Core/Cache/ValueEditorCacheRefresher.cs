using System.Linq;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Cache
{
    public class ValueEditorCacheRefresher :
        INotificationHandler<DataTypeSavedNotification>,
        INotificationHandler<DataTypeDeletedNotification>
    {
        private readonly IValueEditorCache _valueEditorCache;

        public ValueEditorCacheRefresher(IValueEditorCache valueEditorCache) => _valueEditorCache = valueEditorCache;

        public void Handle(DataTypeSavedNotification notification) =>
            _valueEditorCache.ClearCache(notification.SavedEntities.Select(x => x.Id));

        public void Handle(DataTypeDeletedNotification notification) =>
            _valueEditorCache.ClearCache(notification.DeletedEntities.Select(x => x.Id));
    }
}
