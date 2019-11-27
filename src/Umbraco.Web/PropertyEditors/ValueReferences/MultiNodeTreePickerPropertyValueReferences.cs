using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ValueReferences
{
    public class MultiNodeTreePickerPropertyValueReferences : IDataValueReference
    {
        public IEnumerable<UmbracoEntityReference> GetReferences(object value)
        {
            var asString = value == null ? string.Empty : value is string str ? str : value.ToString();

            var udiPaths = asString.Split(',');
            foreach (var udiPath in udiPaths)
                if (Udi.TryParse(udiPath, out var udi))
                    yield return new UmbracoEntityReference(udi);

        }

        public bool IsForEditor(IDataEditor dataEditor) => dataEditor.Alias.InvariantEquals(Constants.PropertyEditors.Aliases.MultiNodeTreePicker);
    }
}
