namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of an <see cref="IContentService" /> sort-children operation.
/// </summary>
public enum ContentSortChildrenOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     No children were supplied to sort, so no work was done.
    /// </summary>
    NoOperation,
}
