using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// A property editor to allow multiple checkbox selection of pre-defined items.
    /// </summary>
    /// <remarks>
    /// Due to remaining backwards compatible, this stores the id of the checkbox items in the database 
    /// as INT and we have logic in here to ensure it is formatted correctly including ensuring that the string value is published
    /// in cache and not the int ID.
    /// </remarks>
    [PropertyEditor(Constants.PropertyEditors.CheckBoxListAlias, "Checkbox list", "checkboxlist")]
    public class CheckBoxListPropertyEditor : PropertyEditor
    {

        /// <summary>
        /// Return a custom pre-value editor
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// We are just going to re-use the ValueListPreValueEditor
        /// </remarks>
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new ValueListPreValueEditor();
        }

        /// <summary>
        /// We need to override the value editor so that we can ensure the string value is published in cache and not the integer ID value.
        /// </summary>
        /// <returns></returns>
        protected override PropertyValueEditor CreateValueEditor()
        {
            return new PublishValuesMultipleValueEditor(false, base.CreateValueEditor());
        }

    }
}