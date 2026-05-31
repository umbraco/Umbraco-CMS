using System.Text.Json.Nodes;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property editor for configuration-less decimal properties.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.PlainDecimal,
    ValueEditorIsReusable = true,
    ValueType = ValueTypes.Decimal)]
public class PlainDecimalPropertyEditor : DataEditor, IValueSchemaProvider
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PlainDecimalPropertyEditor" /> class.
    /// </summary>
    public PlainDecimalPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;

    /// <inheritdoc />
    public Type? GetValueType(object? configuration) => typeof(decimal?);

    /// <inheritdoc />
    public JsonObject? GetValueSchema(object? configuration) => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["type"] = new JsonArray("number", "null"),
        ["description"] = "Plain decimal number value",
    };
}
