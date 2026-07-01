using System.Text.Json.Nodes;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property editor for configuration-less JSON properties.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.PlainJson,
    ValueEditorIsReusable = true,
    ValueType = ValueTypes.Json)]
public class PlainJsonPropertyEditor : DataEditor, IValueSchemaProvider
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PlainJsonPropertyEditor" /> class.
    /// </summary>
    public PlainJsonPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;

    /// <inheritdoc />
    public Type? GetValueType(object? configuration) => typeof(object);

    /// <inheritdoc />
    public JsonObject? GetValueSchema(object? configuration) => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["description"] = "Any valid JSON value",
    };
}
