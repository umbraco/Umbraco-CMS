namespace Umbraco.Core.Cache
{

    /// <summary>
    /// Constants storing cache keys used in caching
    /// </summary>
    public static class CacheKeys
    {
        public const string MediaCacheKey = "UL_GetMedia";

        public const string MacroCacheKey = "UmbracoMacroCache";
        public const string MacroHtmlCacheKey = "macroHtml_";
        public const string MacroControlCacheKey = "macroControl_";
        public const string MacroHtmlDateAddedCacheKey = "macroHtml_DateAdded_";
        public const string MacroControlDateAddedCacheKey = "macroControl_DateAdded_";

        public const string MemberCacheKey = "UL_GetMember";

        public const string TemplateCacheKey = "template";

        public const string UserCacheKey = "UmbracoUser";

        public const string ContentTypeCacheKey = "UmbracoContentType";

        public const string ContentTypePropertiesCacheKey = "ContentType_PropertyTypes_Content:";
        
        public const string PropertyTypeCacheKey = "UmbracoPropertyTypeCache";

        public const string LanguageCacheKey = "UmbracoLanguageCache";

        public const string DomainCacheKey = "UmbracoDomainList";

        public const string StylesheetCacheKey = "UmbracoStylesheet";
        public const string StylesheetPropertyCacheKey = "UmbracoStylesheetProperty";

    }
}