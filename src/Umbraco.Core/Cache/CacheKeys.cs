namespace Umbraco.Cms.Core.Cache
{
    /// <summary>
    /// Constants storing cache keys used in caching
    /// </summary>
    public static class CacheKeys
    {
        public const string ApplicationsCacheKey = "ApplicationCache"; // used by SectionService

        // TODO: this one can probably be removed
        public const string TemplateFrontEndCacheKey = "template";

        public const string MacroContentCacheKey = "macroContent_"; // used in MacroRenderers
        public const string MacroFromAliasCacheKey = "macroFromAlias_";

        public const string UserGroupGetByAliasCacheKeyPrefix = "UserGroupRepository_GetByAlias_";
    }
}
