namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a content query operation.
/// </summary>
public enum ContentQueryOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The specified content item was not found.
    /// </summary>
    ContentNotFound,

    /// <summary>
    ///     An unknown error occurred during the operation.
    /// </summary>
    Unknown,
}
