using System.Collections.Generic;
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
    [PropertyEditor(Constants.PropertyEditors.DropdownlistMultiplePublishKeysAlias, "Dropdown list multiple, publish keys", "dropdown")]
    public class DropDownMultipleWithKeysPropertyEditor : DropDownPropertyEditor
    {
        protected override PropertyValueEditor CreateValueEditor()
        {
            return new PublishValuesMultipleValueEditor(true, base.CreateValueEditor());
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
        internal class DropDownMultiplePreValueEditor : ValueListPreValueEditor
        {
            public DropDownMultiplePreValueEditor()
            {
                //add the multiple field, we'll make it hidden so it is not seen in the pre-value editor
                Fields.Add(new PreValueField
                    {
                        Key = "multiple",
                        Name = "multiple",
                        View = "hidden",
                        HideLabel = true
                    });                
            }

            /// <summary>
            /// Always
            /// </summary>
            /// <param name="defaultPreVals"></param>
            /// <param name="persistedPreVals"></param>
            /// <returns></returns>
            public override IDictionary<string, object> ConvertDbToEditor(IDictionary<string, object> defaultPreVals, PreValueCollection persistedPreVals)
            {
                var returnVal = base.ConvertDbToEditor(defaultPreVals, persistedPreVals);
                //always add the multiple param to true
                returnVal["multiple"] = "1";
                return returnVal;
            }
        }

    }
}