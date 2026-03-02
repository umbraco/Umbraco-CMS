using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents a single PATCH operation using Umbraco's path syntax.
/// </summary>
public class PatchOperationRequestModel
{
    /// <summary>
    /// The operation type: "replace", "add", or "remove".
    /// </summary>
    [Required]
    public string Op { get; set; } = string.Empty;

    /// <summary>
    /// Path expression identifying the target location using Umbraco's extended JSON Pointer syntax.
    /// <para>
    /// Examples:
    /// <list type="bullet">
    ///   <item><c>/variants[culture=en-US,segment=null]/name</c></item>
    ///   <item><c>/values[alias=title,culture=en-US,segment=null]/value</c></item>
    ///   <item><c>/values[alias=blocks,culture=null,segment=null]/value/contentData/-</c> (append to array)</item>
    /// </list>
    /// </para>
    /// </summary>
    [Required]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The value to set. Required for "replace" and "add" operations, omitted for "remove".
    /// </summary>
    public object? Value { get; set; }
}
