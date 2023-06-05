using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.Validators;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents an integer property and parameter editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.Integer,
    EditorType.PropertyValue | EditorType.MacroParameter,
    "Numeric",
    "integer",
    ValueType = ValueTypes.Integer,
    ValueEditorIsReusable = true)]
public class IntegerPropertyEditor : DataEditor
{
    public IntegerPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory) =>
        SupportsReadOnly = true;

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor()
    {
        IDataValueEditor editor = base.CreateValueEditor();
        editor.Validators.Add(new IntegerValidator()); // ensure the value is validated
        return editor;
    }

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() => new IntegerConfigurationEditor();
}
