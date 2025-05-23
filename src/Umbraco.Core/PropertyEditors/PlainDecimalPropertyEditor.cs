namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property editor for configuration-less decimal properties.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.PlainDecimal,
    ValueEditorIsReusable = true,
    ValueType = ValueTypes.Decimal)]
public class PlainDecimalPropertyEditor : DataEditor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PlainDecimalPropertyEditor" /> class.
    /// </summary>
    public PlainDecimalPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;
}
