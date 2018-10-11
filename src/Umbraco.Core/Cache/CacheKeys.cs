namespace Umbraco.Core.Cache
{
    /// <summary>
    /// Constants storing cache keys used in caching
    /// </summary>
    public static class CacheKeys
    {
        public const string ApplicationTreeCacheKey = "ApplicationTreeCache"; // used by ApplicationTreeService
        public const string ApplicationsCacheKey = "ApplicationCache"; // used by SectionService

        public const string TemplateFrontEndCacheKey = "template"; // fixme usage?

        public const string MacroContentCacheKey = "macroContent_"; // used in MacroRenderers
    }
}
