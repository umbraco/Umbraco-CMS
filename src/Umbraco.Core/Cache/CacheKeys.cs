namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Constants storing cache keys used in caching.
/// </summary>
public static class CacheKeys
{
    /// <summary>
    ///     Cache key for applications (sections) cache, used by SectionService.
    /// </summary>
    public const string ApplicationsCacheKey = "ApplicationCache";

    /// <summary>
    ///     Cache key prefix for template front-end cache.
    /// </summary>
    // TODO: this one can probably be removed
    public const string TemplateFrontEndCacheKey = "template";

    /// <summary>
    ///     Cache key prefix for user group lookup by alias.
    /// </summary>
    public const string UserGroupGetByAliasCacheKeyPrefix = "UserGroupRepository_GetByAlias_";

    /// <summary>
    ///     Cache key prefix for user's content start nodes.
    /// </summary>
    public const string UserAllContentStartNodesPrefix = "AllContentStartNodes";

    /// <summary>
    ///     Cache key prefix for user's media start nodes.
    /// </summary>
    public const string UserAllMediaStartNodesPrefix = "AllMediaStartNodes";

    /// <summary>
    ///     Cache key prefix for user's element start nodes.
    /// </summary>
    public const string UserAllElementStartNodesPrefix = "AllElementStartNodes";

    /// <summary>
    ///     Cache key prefix for user's media start node paths.
    /// </summary>
    public const string UserMediaStartNodePathsPrefix = "MediaStartNodePaths";

    /// <summary>
    ///     Cache key prefix for user's content start node paths.
    /// </summary>
    public const string UserContentStartNodePathsPrefix = "ContentStartNodePaths";

    /// <summary>
    ///     Cache key prefix for user's elemtent start node paths.
    /// </summary>
    public const string UserElementStartNodePathsPrefix = "ElementStartNodePaths";

    /// <summary>
    ///     Cache key for content recycle bin.
    /// </summary>
    public const string ContentRecycleBinCacheKey = "recycleBin_content";

    /// <summary>
    ///     Cache key for media recycle bin.
    /// </summary>
    public const string MediaRecycleBinCacheKey = "recycleBin_media";

    /// <summary>
    ///     Cache key prefix for preview (draft) property cache values.
    /// </summary>
    public const string PreviewPropertyCacheKeyPrefix = "Cache.Property.CacheValues[D:";

    /// <summary>
    ///     Cache key prefix for published property cache values.
    /// </summary>
    public const string PropertyCacheKeyPrefix = "Cache.Property.CacheValues[P:";

    /// <summary>
    ///     Cache key prefix for member username lookups.
    /// </summary>
    public const string MemberUserNameCachePrefix = "uRepo_userNameKey+";
}
