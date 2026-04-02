// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property editor for configuration-less date/time properties.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.PlainDateTime,
    ValueEditorIsReusable = true,
    ValueType = ValueTypes.DateTime)]
public class PlainDateTimePropertyEditor : DataEditor, IValueSchemaProvider
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PlainDateTimePropertyEditor" /> class.
    /// </summary>
    public PlainDateTimePropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;

    /// <inheritdoc />
    public Type? GetValueType(object? configuration) => typeof(DateTime?);

    /// <inheritdoc />
    public JsonObject? GetValueSchema(object? configuration) => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["type"] = new JsonArray("string", "null"),
        ["format"] = "date-time",
        ["description"] = "ISO 8601 date-time string",
    };
}
