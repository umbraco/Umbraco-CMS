using System.Text.Json.Nodes;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Represents the schema information for a property editor value.
/// </summary>
public sealed class PropertyValueSchema
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyValueSchema"/> class.
    /// </summary>
    /// <param name="valueType">The CLR type of stored values.</param>
    /// <param name="jsonSchema">The JSON Schema describing the value structure.</param>
    public PropertyValueSchema(Type? valueType, JsonObject? jsonSchema)
    {
        ValueType = valueType;
        JsonSchema = jsonSchema;
    }

    /// <summary>
    /// Gets the CLR type of stored values, or <c>null</c> if the type cannot be determined.
    /// </summary>
    public Type? ValueType { get; }

    /// <summary>
    /// Gets the JSON Schema (draft 2020-12) describing the value structure, or <c>null</c> if not provided.
    /// </summary>
    public JsonObject? JsonSchema { get; }
}
