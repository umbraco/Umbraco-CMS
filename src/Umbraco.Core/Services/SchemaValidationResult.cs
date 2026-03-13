namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Represents a validation error from JSON Schema validation.
/// </summary>
public sealed class SchemaValidationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaValidationResult"/> class.
    /// </summary>
    /// <param name="message">The validation error message.</param>
    /// <param name="path">The JSON path to the invalid value.</param>
    /// <param name="keyword">The JSON Schema keyword that failed validation.</param>
    public SchemaValidationResult(string message, string? path = null, string? keyword = null)
    {
        Message = message;
        Path = path;
        Keyword = keyword;
    }

    /// <summary>
    /// Gets the validation error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the JSON path to the invalid value (e.g., "$.items[0].name").
    /// </summary>
    public string? Path { get; }

    /// <summary>
    /// Gets the JSON Schema keyword that failed validation (e.g., "type", "required", "minLength").
    /// </summary>
    public string? Keyword { get; }
}
