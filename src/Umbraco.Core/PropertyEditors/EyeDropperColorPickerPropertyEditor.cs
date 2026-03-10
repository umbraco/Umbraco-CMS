namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property editor for selecting colors using an eye dropper.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.ColorPickerEyeDropper,
    ValueEditorIsReusable = true)]
public class EyeDropperColorPickerPropertyEditor : DataEditor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EyeDropperColorPickerPropertyEditor" /> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">The data value editor factory.</param>
    public EyeDropperColorPickerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;
}
