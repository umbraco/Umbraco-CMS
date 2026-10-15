using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for the single media picker value editor.
/// </summary>
public class SingleMediaPickerConfigurationEditor : ConfigurationEditor<SingleMediaPickerConfiguration>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SingleMediaPickerConfigurationEditor" /> class.
    /// </summary>
    /// <param name="ioHelper">An <see cref="IIOHelper" /> used for file and path operations.</param>
    public SingleMediaPickerConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
