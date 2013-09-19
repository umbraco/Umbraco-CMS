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
        /// We are just going to re-use the ValueListPreValueEditor
        /// </remarks>
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new ColorListPreValueEditor();
        }

    }
}