namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
/// Represents a single PATCH operation in the domain layer.
/// </summary>
public class PatchOperationModel
{
    /// <summary>
    /// Gets or sets the operation type.
    /// </summary>
    public PatchOperationType Op { get; set; }

    /// <summary>
    /// Gets or sets the JSONPath expression identifying the target location.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value to set. Required for Replace and Add operations, null for Remove.
    /// </summary>
    public object? Value { get; set; }
}

/// <summary>
/// Defines the supported PATCH operation types.
/// </summary>
public enum PatchOperationType
{
    /// <summary>
    /// Replace an existing value at the target location.
    /// </summary>
    Replace,

    /// <summary>
    /// Add a new value at the target location.
    /// </summary>
    Add,

    /// <summary>
    /// Remove the value at the target location.
    /// </summary>
    Remove
}
