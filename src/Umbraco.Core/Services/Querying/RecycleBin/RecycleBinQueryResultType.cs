namespace Umbraco.Cms.Core.Services.Querying.RecycleBin;

/// <summary>
/// Represents the result types for recycle bin query operations.
/// </summary>
/// <remarks>
/// Values less than 10 indicate success, values 10 or greater indicate failure.
/// </remarks>
public enum RecycleBinQueryResultType
{
    /// <summary>
    /// The operation completed successfully.
    /// </summary>
    Success = 0,

    /// <summary>
    /// The original parent of the trashed item was the root node.
    /// </summary>
    ParentIsRoot = 2,

    /// <summary>
    /// The specified item was not found.
    /// </summary>
    NotFound = 11,

    /// <summary>
    /// The specified item is not in the recycle bin (not trashed).
    /// </summary>
    NotTrashed = 12,

    /// <summary>
    /// No parent recycle relation was found for the trashed item.
    /// </summary>
    NoParentRecycleRelation = 13,

    /// <summary>
    /// The original parent of the trashed item was not found.
    /// </summary>
    ParentNotFound = 14,

    /// <summary>
    /// The original parent of the trashed item is also in the recycle bin.
    /// </summary>
    ParentIsTrashed = 15,
}
