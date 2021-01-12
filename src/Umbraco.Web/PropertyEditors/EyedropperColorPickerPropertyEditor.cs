using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(
        Constants.PropertyEditors.Aliases.EyedropperColorPicker,
        "Eyedropper Color Picker",
        "eyedropper",
        Icon = "icon-colorpicker",
        Group = Constants.PropertyEditors.Groups.Pickers)]
    public class EyedropperColorPickerPropertyEditor : DataEditor
    {
        public EyedropperColorPickerPropertyEditor(ILogger logger)
            : base(logger)
        { }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new EyedropperColorPickerConfigurationEditor();
    }
}
