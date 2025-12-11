namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.IconPicker,
    ValueEditorIsReusable = true)]
public class IconPickerPropertyEditor : DataEditor
{
    public IconPickerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;
}
