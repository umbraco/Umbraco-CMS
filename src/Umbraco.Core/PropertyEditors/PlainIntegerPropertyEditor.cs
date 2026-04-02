using System.Text.Json.Nodes;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property editor for configuration-less integer properties.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.PlainInteger,
    ValueEditorIsReusable = true,
    ValueType = ValueTypes.Integer)]
public class PlainIntegerPropertyEditor : DataEditor, IValueSchemaProvider
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PlainIntegerPropertyEditor" /> class.
    /// </summary>
    public PlainIntegerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;

    /// <inheritdoc />
    public Type? GetValueType(object? configuration) => typeof(int?);

    /// <inheritdoc />
    public JsonObject? GetValueSchema(object? configuration) => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["type"] = new JsonArray("integer", "null"),
        ["description"] = "Plain integer value",
    };
}
