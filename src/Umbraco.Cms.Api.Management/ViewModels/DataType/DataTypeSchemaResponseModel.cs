using System.Text.Json.Nodes;

namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

/// <summary>
/// Represents schema information for a data type's stored values.
/// </summary>
public class DataTypeSchemaResponseModel
{
    /// <summary>
    /// Gets or sets the full name of the CLR type representing stored values.
    /// </summary>
    /// <remarks>
    /// This can be <c>null</c> if the property editor doesn't provide type information
    /// or if the type varies significantly based on configuration.
    /// </remarks>
    public string? ValueTypeName { get; set; }

    /// <summary>
    /// Gets or sets the JSON Schema (draft 2020-12) describing the value structure.
    /// </summary>
    /// <remarks>
    /// This can be <c>null</c> if the property editor doesn't provide schema information.
    /// </remarks>
    public JsonObject? JsonSchema { get; set; }
}
