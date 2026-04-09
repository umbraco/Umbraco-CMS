namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of an API content query operation.
/// </summary>
public enum ApiContentQueryOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The specified filter option was not found.
    /// </summary>
    FilterOptionNotFound,

    /// <summary>
    ///     The specified selector option was not found.
    /// </summary>
    SelectorOptionNotFound,

    /// <summary>
    ///     The specified sort option was not found.
    /// </summary>
    SortOptionNotFound
}
