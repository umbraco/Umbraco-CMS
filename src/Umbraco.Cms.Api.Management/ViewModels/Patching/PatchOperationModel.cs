namespace Umbraco.Cms.Api.Management.ViewModels.Patching;

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
    /// Gets or sets the patch path expression identifying the target location.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value to set. Required for Replace and Add operations, null for Remove.
    /// </summary>
    public object? Value { get; set; }
}
