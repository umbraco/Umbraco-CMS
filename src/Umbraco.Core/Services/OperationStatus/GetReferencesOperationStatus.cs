namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a get references operation.
/// </summary>
public enum GetReferencesOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The specified content item was not found.
    /// </summary>
    ContentNotFound
}
