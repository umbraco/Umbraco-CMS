using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(
        Constants.PropertyEditors.Aliases.EyeDropperColorPicker,
        "EyeDropper Color Picker",
        "eyedropper",
        Icon = "icon-colorpicker",
        Group = Constants.PropertyEditors.Groups.Pickers)]
    public class EyeDropperColorPickerPropertyEditor : DataEditor
    {
        public EyeDropperColorPickerPropertyEditor(ILogger logger)
            : base(logger)
        { }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new EyeDropperColorPickerConfigurationEditor();
    }
}
