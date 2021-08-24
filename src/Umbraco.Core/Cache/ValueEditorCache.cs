using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Cache
{
    public class ValueEditorCache : IValueEditorCache,
        INotificationHandler<DataTypeSavedNotification>,
        INotificationHandler<DataTypeDeletedNotification>
    {
        private readonly Dictionary<string, Dictionary<int, IDataValueEditor>> _valueEditorCache;
        private readonly object _dictionaryLocker;

        public ValueEditorCache()
        {
            _valueEditorCache = new Dictionary<string, Dictionary<int, IDataValueEditor>>();
            _dictionaryLocker = new object();
        }

        public IDataValueEditor GetValueEditor(IDataEditor editor, IDataType dataType)
        {
            // Lock just in case multiple threads uses the cache at the same time.
            lock (_dictionaryLocker)
            {
                // We try and get the dictionary based on the IDataEditor alias,
                // this is here just in case a data type can have more than one value data editor.
                // If this is not the case this could be simplified quite a bit, by just using the inner dictionary only.
                IDataValueEditor valueEditor;
                if (_valueEditorCache.TryGetValue(editor.Alias, out Dictionary<int, IDataValueEditor> dataEditorCache))
                {
                    if (dataEditorCache.TryGetValue(dataType.Id, out valueEditor))
                    {
                        return valueEditor;
                    }

                    valueEditor = editor.GetValueEditor(dataType.Configuration);
                    dataEditorCache[dataType.Id] = valueEditor;
                    return valueEditor;
                }

                valueEditor = editor.GetValueEditor(dataType.Configuration);
                _valueEditorCache[editor.Alias] = new Dictionary<int, IDataValueEditor> { [dataType.Id] = valueEditor };
                return valueEditor;
            }
        }

        public void Handle(DataTypeSavedNotification notification) =>
            ClearCache(notification.SavedEntities.Select(x => x.Id));

        public void Handle(DataTypeDeletedNotification notification) =>
            ClearCache(notification.DeletedEntities.Select(x => x.Id));

        private void ClearCache(IEnumerable<int> dataTypeIds)
        {
            lock (_dictionaryLocker)
            {
                // If a datatype is saved or deleted we have to clear any value editors based on their ID from the cache,
                // since it could mean that their configuration has changed.
                foreach (var id in dataTypeIds)
                {
                    foreach (Dictionary<int, IDataValueEditor> editors in _valueEditorCache.Values)
                    {
                        editors.Remove(id);
                    }
                }
            }
        }
    }
}
