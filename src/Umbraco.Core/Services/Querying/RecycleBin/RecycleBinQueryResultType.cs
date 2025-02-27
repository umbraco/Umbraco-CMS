namespace Umbraco.Cms.Core.Services.Querying.RecycleBin;

/// <summary>
/// &lt;10 = Success.
/// </summary>
public enum RecycleBinQueryResultType
{
    Success = 0,
    ParentIsRoot = 2,
    NotFound = 11,
    NotTrashed = 12,
    NoParentRecycleRelation = 13,
    ParentNotFound = 14,
    ParentIsTrashed = 15,
}
