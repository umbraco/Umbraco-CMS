using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a media picker property editor holding any number of media items.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.MediaPicker3,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = true)]
public class MediaPicker3PropertyEditor : MediaPickerPropertyEditorBase
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaPicker3PropertyEditor" /> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">Factory used to create data value editors for the media picker property editor.</param>
    /// <param name="ioHelper">Helper for IO operations, such as file and path handling.</param>
    public MediaPicker3PropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory)
        => _ioHelper = ioHelper;

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new MediaPicker3ConfigurationEditor(_ioHelper);
}
