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
    /// One or more PATCH operations were invalid (invalid JSONPath syntax, unsupported operation type, missing required value).
    /// </summary>
    InvalidOperation,

    /// <summary>
    /// One or more cultures specified in operation paths are not valid or not configured.
    /// </summary>
    InvalidCulture,

    /// <summary>
    /// The target document could not be found.
    /// </summary>
    NotFound,

    /// <summary>
    /// The document's content type could not be found.
    /// </summary>
    ContentTypeNotFound,

    /// <summary>
    /// One or more property types specified in operations do not exist on the content type.
    /// </summary>
    PropertyTypeNotFound,
}
