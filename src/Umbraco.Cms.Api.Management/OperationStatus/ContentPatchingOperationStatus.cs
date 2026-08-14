namespace Umbraco.Cms.Api.Management.OperationStatus;

/// <summary>
/// Operation status for PATCH operations at the API layer.
/// This is distinct from ContentEditingOperationStatus which is for service layer operations.
/// </summary>
public enum ContentPatchingOperationStatus
{
    /// <summary>
    /// The operation was successful.
    /// </summary>
    Success,

    /// <summary>
    /// One or more PATCH operations were invalid (invalid path syntax, unsupported operation type, missing required value).
    /// </summary>
    InvalidOperation,

    /// <summary>
    /// The target document could not be found.
    /// </summary>
    NotFound,
}
