namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

/// <summary>
/// Represents a single validation error from schema validation.
/// </summary>
public class SchemaValidationResultResponseModel
{
    /// <summary>
    /// Gets or sets the validation error message.
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Gets or sets the JSON path where the error occurred (e.g., "$.items[0].name").
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets the JSON Schema keyword that failed (e.g., "type", "required", "minimum").
    /// </summary>
    public string? Keyword { get; set; }
}
