namespace Umbraco.Cms.Core.PropertyEditors.ParameterEditors;

[DataEditor(
    "propertyTypePickerMultiple",
    EditorType.MacroParameter,
    "Multiple Property Type Picker",
    "entitypicker")]
public class MultiplePropertyTypeParameterEditor : DataEditor
{
    public MultiplePropertyTypeParameterEditor(
        IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
    {
        // configure
        DefaultConfiguration.Add("multiple", "1");
        DefaultConfiguration.Add("entityType", "PropertyType");

        // don't publish the id for a property type, publish its alias
        DefaultConfiguration.Add("publishBy", "alias");
        SupportsReadOnly = true;
    }
}
