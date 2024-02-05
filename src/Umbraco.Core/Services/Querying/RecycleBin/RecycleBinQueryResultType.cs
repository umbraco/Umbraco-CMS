namespace Umbraco.Cms.Core.Services.Querying.RecycleBin;

/// <summary>
/// < 10 = Success
/// </summary>
public enum RecycleBinQueryResultType
{
    Success = 0,
    ParentUnavailable = 1,
    NotFound = 11,
    NotTrashed = 12,
    NoParentRecycleRelation = 13
}
