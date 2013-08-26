using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// A property editor to allow multiple selection of pre-defined items
    /// </summary>
    /// <remarks>
    /// Due to maintaining backwards compatibility this data type stores the value as a string which is a comma separated value of the 
    /// ids of the individual items so we have logic in here to deal with that.
    /// </remarks>
    [PropertyEditor(Constants.PropertyEditors.DropDownListMultiple, "Dropdown list multiple", "dropdown")]
    public class DropDownMultiplePropertyEditor : DropDownPropertyEditor
    {
        protected override ValueEditor CreateValueEditor()
        {
            return new DropDownMultipleValueEditor(base.CreateValueEditor());
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new DropDownMultiplePreValueEditor();
        }

        /// <summary>
        /// A pre-value editor for the 'drop down list multiple' property editor that ensures that 'multiple' is saved for the config in the db but is not 
        /// rendered as a pre-value field.
        /// </summary>
        /// <remarks>
        /// This is mostly to maintain backwards compatibility with old property editors. Devs can now simply use the Drop down property editor and check the multiple pre-value checkbox
        /// </remarks>
        internal class DropDownMultiplePreValueEditor : DropDownPreValueEditor
        {
            public DropDownMultiplePreValueEditor()
            {
                var fields = CreatePreValueFields();
                //add the multiple field, we'll make it hidden so it is not seen in the pre-value editor
                fields.Add(new PreValueField
                {
                    Key = "multiple",
                    Name = "multiple",
                    View = "hidden"
                });
                Fields = fields;
            }

            /// <summary>
            /// Always
            /// </summary>
            /// <param name="defaultPreVals"></param>
            /// <param name="persistedPreVals"></param>
            /// <returns></returns>
            public override IDictionary<string, object> FormatDataForEditor(IDictionary<string, object> defaultPreVals, PreValueCollection persistedPreVals)
            {
                var returnVal = base.FormatDataForEditor(defaultPreVals, persistedPreVals);
                //always add the multiple param to true
                returnVal["multiple"] = "1";
                return returnVal;
            }
        }

        /// <summary>
        /// Custom value editor to handle posted json data and to return json data for the multiple selected items.
        /// </summary>
        internal class DropDownMultipleValueEditor : ValueEditorWrapper
        {
            public DropDownMultipleValueEditor(ValueEditor wrapped)
                : base(wrapped)
            {
            }

            /// <summary>
            /// Override so that we can return a json array to the editor for multi-select values
            /// </summary>
            /// <param name="dbValue"></param>
            /// <returns></returns>
            public override object FormatDataForEditor(object dbValue)
            {
                var delimited = base.FormatDataForEditor(dbValue).ToString();
                return delimited.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }

            /// <summary>
            /// When multiple values are selected a json array will be posted back so we need to format for storage in 
            /// the database which is a comma separated ID value
            /// </summary>
            /// <param name="editorValue"></param>
            /// <param name="currentValue"></param>
            /// <returns></returns>
            public override object FormatDataForPersistence(Core.Models.Editors.ContentPropertyData editorValue, object currentValue)
            {
                var json = editorValue.Value as JArray;
                if (json == null)
                {
                    return null;
                }

                var values = json.Select(item => item.Value<string>()).ToList();
                //change to delimited
                return string.Join(",", values);
            }
        }

    }

    

    
}