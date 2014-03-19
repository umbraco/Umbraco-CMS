using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.ColorPickerAlias, "Color Picker", "colorpicker")]
    public class ColorPickerPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// Return a custom pre-value editor
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// ColorListPreValueEditor uses the ValueListPreValueEditor with a custom view and controller.
        /// </remarks>
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new ColorListPreValueEditor();
        }

    }
}