namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.UserPicker,
    "User Picker",
    "userpicker",
    ValueType = ValueTypes.Integer,
    Group = Constants.PropertyEditors.Groups.People,
    Icon = Constants.Icons.User,
    ValueEditorIsReusable = true)]
public class UserPickerPropertyEditor : DataEditor
{
    public UserPickerPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory) =>
        SupportsReadOnly = true;

    protected override IConfigurationEditor CreateConfigurationEditor() => new UserPickerConfiguration();
}
