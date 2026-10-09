using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a media picker property editor holding a single media item.
/// </summary>
/// <remarks>
/// Stores the same value as <see cref="MediaPicker3PropertyEditor" /> and is edited the same way; the two differ in
/// the shape of the value they yield, and in how many items may be picked.
/// </remarks>
[DataEditor(
    Constants.PropertyEditors.Aliases.SingleMediaPicker,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = true)]
public class SingleMediaPickerPropertyEditor : MediaPickerPropertyEditorBase
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SingleMediaPickerPropertyEditor" /> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">Factory used to create data value editors for the media picker property editor.</param>
    /// <param name="ioHelper">Helper for IO operations, such as file and path handling.</param>
    public SingleMediaPickerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory)
        => _ioHelper = ioHelper;

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new SingleMediaPickerConfigurationEditor(_ioHelper);
}
