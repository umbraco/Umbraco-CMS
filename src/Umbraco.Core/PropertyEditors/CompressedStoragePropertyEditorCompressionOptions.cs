using System.Collections.Concurrent;
using System.Linq;
using Umbraco.Core.Services;

namespace Umbraco.Core.PropertyEditors
{

    /// <summary>
    /// Ensures all property types that have an editor storing a complex value are compressed
    /// </summary>
    public class CompressedStoragePropertyEditorCompressionOptions : IPropertyCompressionOptions
    {
        private readonly IContentTypeService _contentTypeService;
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly ConcurrentDictionary<(int, string), string> _editorValueTypes = new ConcurrentDictionary<(int, string), string>();

        public CompressedStoragePropertyEditorCompressionOptions(PropertyEditorCollection propertyEditors)
        {
            _propertyEditors = propertyEditors;
        }

        public bool IsCompressed(int contentTypeId, string alias)
        {
            return false;
            //var valueType = _editorValueTypes.GetOrAdd((contentTypeId, alias), x =>
            //{
            //    var ct = _contentTypeService.Get(contentTypeId);
            //    if (ct == null) return null;

            //    var propertyType = ct.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == alias);
            //    if (propertyType == null) return null;

            //    if (!_propertyEditors.TryGet(propertyType.PropertyEditorAlias, out var propertyEditor)) return null;

            //    var editor = propertyEditor.GetValueEditor();
            //    if (editor == null) return null;

            //    return editor.ValueType;
            //});

            //return valueType == ValueTypes.Json || valueType == ValueTypes.Xml || valueType == ValueTypes.Text;
        }
    }
}
