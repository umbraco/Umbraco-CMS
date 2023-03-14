namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Specifies how a repository cache policy should cache entities.
/// </summary>
public class RepositoryCachePolicyOptions
{
    /// <summary>
    ///     Ctor - sets GetAllCacheValidateCount = true
    /// </summary>
    public RepositoryCachePolicyOptions(Func<int> performCount)
    {
        PerformCount = performCount;
        GetAllCacheValidateCount = true;
        GetAllCacheAllowZeroCount = false;
    }

    /// <summary>
    ///     Ctor - sets GetAllCacheValidateCount = false
    /// </summary>
    public RepositoryCachePolicyOptions()
    {
        PerformCount = null;
        GetAllCacheValidateCount = false;
        GetAllCacheAllowZeroCount = false;
    }

    /// <summary>
    ///     Callback required to get count for GetAllCacheValidateCount
    /// </summary>
    public Func<int>? PerformCount { get; set; }

    /// <summary>
    ///     True/false as to validate the total item count when all items are returned from cache, the default is true but this
    ///     means that a db lookup will occur - though that lookup will probably be significantly less expensive than the
    ///     normal
    ///     GetAll method.
    /// </summary>
    /// <remarks>
    ///     setting this to return false will improve performance of GetAll cache with no params but should only be used
    ///     for specific circumstances
    /// </remarks>
    public bool GetAllCacheValidateCount { get; set; }

    /// <summary>
    ///     True if the GetAll method will cache that there are zero results so that the db is not hit when there are no
    ///     results found
    /// </summary>
    public bool GetAllCacheAllowZeroCount { get; set; }
}
