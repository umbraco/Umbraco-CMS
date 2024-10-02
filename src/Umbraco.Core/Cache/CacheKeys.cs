namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Constants storing cache keys used in caching
/// </summary>
public static class CacheKeys
{
    public const string ApplicationsCacheKey = "ApplicationCache"; // used by SectionService

    // TODO: this one can probably be removed
    public const string TemplateFrontEndCacheKey = "template";

    public const string UserGroupGetByAliasCacheKeyPrefix = "UserGroupRepository_GetByAlias_";

    public const string UserAllContentStartNodesPrefix = "AllContentStartNodes";
    public const string UserAllMediaStartNodesPrefix = "AllMediaStartNodes";
    public const string UserMediaStartNodePathsPrefix = "MediaStartNodePaths";
    public const string UserContentStartNodePathsPrefix = "ContentStartNodePaths";

    public const string ContentRecycleBinCacheKey = "recycleBin_content";
    public const string MediaRecycleBinCacheKey = "recycleBin_media";

    public const string PreviewPropertyCacheKeyPrefix = "Cache.Property.CacheValues[D:";
    public const string PropertyCacheKeyPrefix = "Cache.Property.CacheValues[P:";
}
