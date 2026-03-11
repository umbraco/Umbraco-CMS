namespace Umbraco.Cms.Api.Management.ViewModels.Patching;

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
    Remove,
}
