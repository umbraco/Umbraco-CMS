using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using static Umbraco.Web.PropertyEditors.NestedContentPropertyEditor;

namespace Umbraco.Web.PropertyEditors.ValueReferences
{
    public class NestedContentPropertyValueReferences : IDataValueReference
    {
        private PropertyEditorCollection _propertyEditors;
        private NestedContentValues _nestedContentValues;

        //ARGH LightInject moaning at me
        public NestedContentPropertyValueReferences(PropertyEditorCollection propertyEditors, IContentTypeService contentTypeService)
        {
            _propertyEditors = propertyEditors;
            _nestedContentValues = new NestedContentValues(contentTypeService);
        }

        public IEnumerable<UmbracoEntityReference> GetReferences(object value)
        {
            var rawJson = value == null ? string.Empty : value is string str ? str : value.ToString();

            var result = new List<UmbracoEntityReference>();

            foreach (var row in _nestedContentValues.GetPropertyValues(rawJson, out _))
            {
                if (row.PropType == null) continue;

                var propEditor = _propertyEditors[row.PropType.PropertyEditorAlias];

                var valueEditor = propEditor?.GetValueEditor();
                if (!(valueEditor is IDataValueReference reference)) continue;

                var val = row.JsonRowValue[row.PropKey]?.ToString();

                var refs = reference.GetReferences(val);

                result.AddRange(refs);
            }

            return result;
        }

        public bool IsForEditor(IDataEditor dataEditor) => dataEditor.Alias.InvariantEquals(Constants.PropertyEditors.Aliases.NestedContent);
    }
}
