using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.DropDownListFlexibleAlias, "Dropdown", "dropdownFlexible", Group = "lists", Icon = "icon-indent")]
    public class DropdownFlexiblePropertyEditor : PropertyEditor
    {
        private static readonly string _multipleKey = "multiple";

        /// <summary>
        /// Return a custom pre-value editor
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// We are just going to re-use the ValueListPreValueEditor
        /// </remarks>
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new DropdownFlexiblePreValueEditor();
        }

        /// <summary>
        /// We need to override the value editor so that we can ensure the string value is published in cache and not the integer ID value.
        /// </summary>
        /// <returns></returns>
        protected override PropertyValueEditor CreateValueEditor()
        {
            return new PublishValuesMultipleValueEditor(false, base.CreateValueEditor());
        }

        internal class DropdownFlexiblePreValueEditor : ValueListPreValueEditor
        {
            public DropdownFlexiblePreValueEditor()
            {
                Fields.Insert(0, new PreValueField
                {
                    Key = "multiple",
                    Name = "Enable multiple choice",
                    Description = "When checked, the dropdown will be a select multiple / combo box style dropdown",
                    View = "boolean"
                });
            }

            public override IDictionary<string, PreValue> ConvertEditorToDb(IDictionary<string, object> editorValue, PreValueCollection currentValue)
            {

                var result = base.ConvertEditorToDb(editorValue, currentValue);

                // get multiple config
                var multipleValue = editorValue[_multipleKey] != null ? editorValue[_multipleKey].ToString() : "0";
                result.Add(_multipleKey, new PreValue(-1, multipleValue));

                return result;
            }

            public override IDictionary<string, object> ConvertDbToEditor(IDictionary<string, object> defaultPreVals, PreValueCollection persistedPreVals)
            {
                // weird way, but as the value stored is 0 or 1 need to do it this way
                string multipleMode = "0";
                if (persistedPreVals != null && persistedPreVals.PreValuesAsDictionary[_multipleKey] != null)
                {
                    multipleMode = persistedPreVals.PreValuesAsDictionary[_multipleKey].Value;

                    // remove from the collection sent to the base multiple values collection
                    persistedPreVals.PreValuesAsDictionary.Remove(_multipleKey);
                }

                var returnVal = base.ConvertDbToEditor(defaultPreVals, persistedPreVals);

                returnVal[_multipleKey] = multipleMode;
                return returnVal;
            }

        }
    }
}
