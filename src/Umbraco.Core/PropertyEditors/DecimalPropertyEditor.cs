using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.Validators;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a decimal property and parameter editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.Decimal,
    EditorType.PropertyValue | EditorType.MacroParameter,
    "Decimal",
    "decimal",
    ValueType = ValueTypes.Decimal,
    ValueEditorIsReusable = true)]
public class DecimalPropertyEditor : DataEditor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DecimalPropertyEditor" /> class.
    /// </summary>
    public DecimalPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory) =>
        SupportsReadOnly = true;

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor()
    {
        IDataValueEditor editor = base.CreateValueEditor();
        editor.Validators.Add(new DecimalValidator());
        return editor;
    }

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() => new DecimalConfigurationEditor();
}
