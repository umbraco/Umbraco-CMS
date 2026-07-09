using System.Text.Json.Nodes;

namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

/// <summary>
/// Represents the schema information for a single data type in a batch response.
/// </summary>
public class DataTypeSchemaItemResponseModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the data type.
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the full name of the CLR type for property values, if available.
    /// </summary>
    public string? ValueTypeName { get; set; }

    /// <summary>
    /// Gets or sets the JSON Schema for property values, if available.
    /// </summary>
    public JsonObject? JsonSchema { get; set; }

    /// <summary>
    /// Gets or sets the error status if the schema could not be retrieved.
    /// Possible values: "DataTypeNotFound", "SchemaNotSupported", or null if successful.
    /// </summary>
    public string? Error { get; set; }
}
