namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
/// Represents the status of a property type operation.
/// </summary>
public enum PropertyTypeOperationStatus
{
    /// <summary>
    /// The property type operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The content type associated with the property type was not found.
    /// </summary>
    ContentTypeNotFound,
}
