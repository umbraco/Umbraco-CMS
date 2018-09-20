using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(Constants.PropertyEditors.Aliases.ColorPicker, "Color Picker", "colorpicker", Icon="icon-colorpicker", Group="Pickers")]
    public class ColorPickerPropertyEditor : DataEditor
    {
        public ColorPickerPropertyEditor(ILogger logger)
            : base(logger)
        { }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new ColorPickerConfigurationEditor();
    }
}
