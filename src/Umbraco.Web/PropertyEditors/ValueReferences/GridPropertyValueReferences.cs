using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ValueReferences
{
    public class GridPropertyValueReferences : IDataValueReference
    {

        /// <summary>
        /// Resolve references from <see cref="IDataValueEditor"/> values
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public IEnumerable<UmbracoEntityReference> GetReferences(object value)
        {
            return Enumerable.Empty<UmbracoEntityReference>();

            //var rawJson = value == null ? string.Empty : value is string str ? str : value.ToString();

            ////TODO FIX SQUIGLES
            //GridPropertyEditor.GridPropertyValueEditor.DeserializeGridValue(rawJson, out var richTextEditorValues, out var mediaValues);

            //foreach (var umbracoEntityReference in richTextEditorValues.SelectMany(x =>                
            //    _richTextPropertyValueEditor.GetReferences(x.Value)))
            //    yield return umbracoEntityReference;

            //foreach (var umbracoEntityReference in mediaValues.SelectMany(x =>
            //    _mediaPickerPropertyValueEditor.GetReferences(x.Value["udi"])))
            //    yield return umbracoEntityReference;
        }

        public bool IsForEditor(IDataEditor dataEditor) => dataEditor.Alias.InvariantEquals(Constants.PropertyEditors.Aliases.Grid);
    }
}
