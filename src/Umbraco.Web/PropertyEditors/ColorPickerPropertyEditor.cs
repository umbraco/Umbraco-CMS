using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.ColorPickerAlias, "Color Picker", "colorpicker")]
    public class ColorPickerPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public ColorPickerPropertyEditor(ILogger logger) : base(logger)
        {
        }

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