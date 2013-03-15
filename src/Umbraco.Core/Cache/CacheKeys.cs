namespace Umbraco.Core.Cache
{

    /// <summary>
    /// Constants storing cache keys used in caching
    /// </summary>
    public static class CacheKeys
    {
        public const string MediaCacheKey = "UL_GetMedia";

        //NOTE: pretty sure this is never used anymore
        internal const string MacroRuntimeCacheKey = "UmbracoRuntimeMacroCache";
        public const string MacroCacheKey = "UmbracoMacroCache";
        public const string MacroHtmlCacheKey = "macroHtml_";
        public const string MacroControlCacheKey = "macroControl_";
        public const string MacroHtmlDateAddedCacheKey = "macroHtml_DateAdded_";
        public const string MacroControlDateAddedCacheKey = "macroControl_DateAdded_";

        public const string MemberCacheKey = "UL_GetMember";

        public const string TemplateCacheKey = "template";

        public const string UserCacheKey = "UmbracoUser";
    }
}