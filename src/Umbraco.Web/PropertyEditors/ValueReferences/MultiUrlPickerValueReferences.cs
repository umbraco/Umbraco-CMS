using Newtonsoft.Json;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ValueReferences
{
    public class MultiUrlPickerValueReferences : IDataValueReference
    {
        public IEnumerable<UmbracoEntityReference> GetReferences(object value)
        {
            var asString = value == null ? string.Empty : value is string str ? str : value.ToString();

            if (string.IsNullOrEmpty(asString)) yield break;

            var links = JsonConvert.DeserializeObject<List<MultiUrlPickerValueEditor.LinkDto>>(asString);
            foreach (var link in links)
                if (link.Udi != null) // Links can be absolute links without a Udi
                    yield return new UmbracoEntityReference(link.Udi);
        }

        public bool IsForEditor(IDataEditor dataEditor) => dataEditor.Alias.InvariantEquals(Constants.PropertyEditors.Aliases.MultiUrlPicker);

    }
}
