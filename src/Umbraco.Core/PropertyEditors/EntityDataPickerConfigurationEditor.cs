using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

internal sealed class EntityDataPickerConfigurationEditor : ConfigurationEditor<EntityDataPickerConfiguration>
{
    public EntityDataPickerConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
