using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents a single PATCH operation following JSON Patch (RFC 6902) semantics with JSONPath.
/// </summary>
public class PatchOperationRequestModel
{
    /// <summary>
    /// The operation type: "replace", "add", or "remove".
    /// </summary>
    [Required]
    public string Op { get; set; } = string.Empty;

    /// <summary>
    /// JSONPath expression identifying the target location (e.g., "$.values[?(@.alias == 'title' &amp;&amp; @.culture == 'en-US')].value").
    /// </summary>
    [Required]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The value to set. Required for "replace" and "add" operations, omitted for "remove".
    /// </summary>
    public object? Value { get; set; }
}
