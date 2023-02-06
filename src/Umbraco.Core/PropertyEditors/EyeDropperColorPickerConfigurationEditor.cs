using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class EyeDropperColorPickerConfigurationEditor : ConfigurationEditor<EyeDropperColorPickerConfiguration>
{
    public EyeDropperColorPickerConfigurationEditor(
        IIOHelper ioHelper,
        IEditorConfigurationParser editorConfigurationParser)
        : base(ioHelper, editorConfigurationParser)
    {
    }
}
