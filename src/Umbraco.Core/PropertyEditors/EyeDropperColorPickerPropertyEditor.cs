using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors
{
    [DataEditor(
        Constants.PropertyEditors.Aliases.ColorPickerEyeDropper,
        EditorType.PropertyValue | EditorType.MacroParameter,
        "Eye Dropper Color Picker",
        "eyedropper",
        Icon = "icon-colorpicker",
        Group = Constants.PropertyEditors.Groups.Pickers)]
    public class EyeDropperColorPickerPropertyEditor : DataEditor
    {
        private readonly IIOHelper _ioHelper;

        public EyeDropperColorPickerPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            IIOHelper ioHelper,
            EditorType type = EditorType.PropertyValue)
            : base(dataValueEditorFactory, type)
        {
            _ioHelper = ioHelper;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new EyeDropperColorPickerConfigurationEditor(_ioHelper);
    }
}
