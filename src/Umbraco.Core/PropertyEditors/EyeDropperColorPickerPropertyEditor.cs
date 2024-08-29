namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.ColorPickerEyeDropper,
    ValueEditorIsReusable = true)]
public class EyeDropperColorPickerPropertyEditor : DataEditor
{
    public EyeDropperColorPickerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;
}
