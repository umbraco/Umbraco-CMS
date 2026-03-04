using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents the configuration editor for the entity data picker property editor.
/// </summary>
internal sealed class EntityDataPickerConfigurationEditor : ConfigurationEditor<EntityDataPickerConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityDataPickerConfigurationEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">The IO helper.</param>
    public EntityDataPickerConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
