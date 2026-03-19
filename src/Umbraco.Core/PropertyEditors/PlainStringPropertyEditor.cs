using System.Text.Json.Nodes;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property editor for configuration-less string properties.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.PlainString,
    ValueEditorIsReusable = true,
    ValueType = ValueTypes.Text)] // NOTE: for ease of use it's called "String", but it's really stored as TEXT
public class PlainStringPropertyEditor : DataEditor, IValueSchemaProvider
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PlainStringPropertyEditor" /> class.
    /// </summary>
    public PlainStringPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;

    /// <inheritdoc />
    public Type? GetValueType(object? configuration) => typeof(string);

    /// <inheritdoc />
    public JsonObject? GetValueSchema(object? configuration) => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["type"] = new JsonArray("string", "null"),
        ["description"] = "Plain text string value",
    };
}
