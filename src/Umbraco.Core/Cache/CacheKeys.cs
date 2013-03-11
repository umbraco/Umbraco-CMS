namespace Umbraco.Core.Cache
{

    /// <summary>
    /// Constants storing cache keys used in caching
    /// </summary>
    public static class CacheKeys
    {
        public const string GetMediaCacheKey = "GetMedia";

        //NOTE: pretty sure this is never used anymore
        internal const string MacroRuntimeCacheKey = "UmbracoRuntimeMacroCache";
        public const string UmbracoMacroCacheKey = "UmbracoMacroCache";

        public const string GetMemberCacheKey = "GetMember";

        public const string TemplateCacheKey = "template";

        public const string UserCacheKey = "UmbracoUser";
    }
}