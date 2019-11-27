using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ValueReferences
{
    public class MediaPickerPropertyValueReferences : IDataValueReference
    {
        public IEnumerable<UmbracoEntityReference> GetReferences(object value)
        {
            var asString = value is string str ? str : value?.ToString();

            if (string.IsNullOrEmpty(asString)) yield break;

            if (Udi.TryParse(asString, out var udi))
                yield return new UmbracoEntityReference(udi);
        }

        public bool IsForEditor(IDataEditor dataEditor) => dataEditor.Alias.InvariantEquals(Constants.PropertyEditors.Aliases.MediaPicker);
    }
}
