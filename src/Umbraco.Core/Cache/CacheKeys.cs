using System;
using System.ComponentModel;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// Constants storing cache keys used in caching
    /// </summary>
    public static class CacheKeys
    {
        public const string ApplicationTreeCacheKey = "ApplicationTreeCache";
        public const string ApplicationsCacheKey = "ApplicationCache";

        [Obsolete("This is no longer used and will be removed from the codebase in the future")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public const string UserTypeCacheKey = "UserTypeCache";

        [Obsolete("This is no longer used and will be removed from the codebase in the future - it is referenced but no cache is stored against this key")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public const string ContentItemCacheKey = "contentItem";

        [UmbracoWillObsolete("This cache key is only used for the legacy 'library' caching, remove in v8")]
        public const string MediaCacheKey = "UL_GetMedia";

        public const string MacroXsltCacheKey = "macroXslt_";

        [UmbracoWillObsolete("This cache key is only used for legacy business logic caching, remove in v8")]
        public const string MacroCacheKey = "UmbracoMacroCache";

        public const string MacroHtmlCacheKey = "macroHtml_";
        public const string MacroControlCacheKey = "macroControl_";
        public const string MacroHtmlDateAddedCacheKey = "macroHtml_DateAdded_";
        public const string MacroControlDateAddedCacheKey = "macroControl_DateAdded_";

        [UmbracoWillObsolete("This cache key is only used for legacy 'library' member caching, remove in v8")]
        public const string MemberLibraryCacheKey = "UL_GetMember";

        [UmbracoWillObsolete("This cache key is only used for legacy business logic caching, remove in v8")]
        public const string MemberBusinessLogicCacheKey = "MemberCacheItem_";

        [UmbracoWillObsolete("This cache key is only used for legacy template business logic caching, remove in v8")]
        public const string TemplateFrontEndCacheKey = "template";

        [Obsolete("This is no longer used and will be removed from the codebase in the future")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public const string TemplateBusinessLogicCacheKey = "UmbracoTemplateCache";

        [Obsolete("This is no longer used and will be removed from the codebase in the future")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public const string UserContextCacheKey = "UmbracoUserContext";

        public const string UserContextTimeoutCacheKey = "UmbracoUserContextTimeout";

        [Obsolete("This is no longer used and will be removed from the codebase in the future")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public const string UserCacheKey = "UmbracoUser";
        
        public const string UserPermissionsCacheKey = "UmbracoUserPermissions";

        [UmbracoWillObsolete("This cache key is only used for legacy business logic caching, remove in v8")]
        public const string ContentTypeCacheKey = "UmbracoContentType";

        [UmbracoWillObsolete("This cache key is only used for legacy business logic caching, remove in v8")]
        public const string ContentTypePropertiesCacheKey = "ContentType_PropertyTypes_Content:";
        
        [UmbracoWillObsolete("This cache key is only used for legacy business logic caching, remove in v8")]
        public const string PropertyTypeCacheKey = "UmbracoPropertyTypeCache";

        [Obsolete("This is no longer used and will be removed from the codebase in the future")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public const string LanguageCacheKey = "UmbracoLanguageCache";

        [Obsolete("This is no longer used and will be removed from the codebase in the future")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public const string DomainCacheKey = "UmbracoDomainList";

        [Obsolete("This is no longer used and will be removed from the codebase in the future")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public const string StylesheetCacheKey = "UmbracoStylesheet";

        [Obsolete("This is no longer used and will be removed from the codebase in the future")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public const string StylesheetPropertyCacheKey = "UmbracoStylesheetProperty";

        [Obsolete("This is no longer used and will be removed from the codebase in the future")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public const string DataTypeCacheKey = "UmbracoDataTypeDefinition";
        public const string DataTypePreValuesCacheKey = "UmbracoPreVal";

        public const string IdToKeyCacheKey = "UI2K";
        public const string KeyToIdCacheKey = "UK2I";
    }
}