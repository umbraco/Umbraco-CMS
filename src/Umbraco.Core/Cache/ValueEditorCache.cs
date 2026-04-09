using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Implements <see cref="IValueEditorCache" /> to cache <see cref="IDataValueEditor" /> instances.
/// </summary>
/// <remarks>
///     This cache stores value editors keyed by data editor alias and data type ID to avoid
///     repeatedly creating value editor instances during request processing.
/// </remarks>
public class ValueEditorCache : IValueEditorCache
{
    private readonly Lock _dictionaryLocker;
    private readonly Dictionary<string, Dictionary<int, IDataValueEditor>> _valueEditorCache;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ValueEditorCache" /> class.
    /// </summary>
    public ValueEditorCache()
    {
        _valueEditorCache = new Dictionary<string, Dictionary<int, IDataValueEditor>>();
        _dictionaryLocker = new Lock();
    }

    /// <inheritdoc />
    public IDataValueEditor GetValueEditor(IDataEditor editor, IDataType dataType)
    {
        // Lock just in case multiple threads uses the cache at the same time.
        lock (_dictionaryLocker)
        {
            // We try and get the dictionary based on the IDataEditor alias,
            // this is here just in case a data type can have more than one value data editor.
            // If this is not the case this could be simplified quite a bit, by just using the inner dictionary only.
            IDataValueEditor? valueEditor;
            if (_valueEditorCache.TryGetValue(editor.Alias, out Dictionary<int, IDataValueEditor>? dataEditorCache))
            {
                if (dataEditorCache.TryGetValue(dataType.Id, out valueEditor))
                {
                    return valueEditor;
                }

                valueEditor = editor.GetValueEditor(dataType.ConfigurationObject);
                dataEditorCache[dataType.Id] = valueEditor;
                return valueEditor;
            }

            valueEditor = editor.GetValueEditor(dataType.ConfigurationObject);
            _valueEditorCache[editor.Alias] = new Dictionary<int, IDataValueEditor> { [dataType.Id] = valueEditor };
            return valueEditor;
        }
    }

    /// <inheritdoc />
    public void ClearCache(IEnumerable<int> dataTypeIds)
    {
        lock (_dictionaryLocker)
        {
            // If a datatype is saved or deleted we have to clear any value editors based on their ID from the cache,
            // since it could mean that their configuration has changed.
            foreach (var id in dataTypeIds)
            {
                foreach (Dictionary<int, IDataValueEditor> editors in _valueEditorCache.Values)
                {
                    if (editors.TryGetValue(id, out IDataValueEditor? editor))
                    {
                        if (editor is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }

                        editors.Remove(id);
                    }
                }
            }
        }
    }
}
