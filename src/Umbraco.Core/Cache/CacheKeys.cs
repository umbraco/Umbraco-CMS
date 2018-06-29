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
        
        [UmbracoWillObsolete("This cache key is only used for legacy business logic caching, remove in v8")]
        public const string MacroCacheKey = "UmbracoMacroCache";

        public const string MacroContentCacheKey = "macroContent_"; // for macro contents

        [UmbracoWillObsolete("This cache key is only used for legacy 'library' member caching, remove in v8")]
        public const string MemberLibraryCacheKey = "UL_GetMember";

        [UmbracoWillObsolete("This cache key is only used for legacy business logic caching, remove in v8")]
        public const string MemberBusinessLogicCacheKey = "MemberCacheItem_";

        [UmbracoWillObsolete("This cache key is only used for legacy template business logic caching, remove in v8")]
        public const string TemplateFrontEndCacheKey = "template";

        public const string UserContextTimeoutCacheKey = "UmbracoUserContextTimeout";

        [UmbracoWillObsolete("This cache key is only used for legacy business logic caching, remove in v8")]
        public const string ContentTypeCacheKey = "UmbracoContentType";

        [UmbracoWillObsolete("This cache key is only used for legacy business logic caching, remove in v8")]
        public const string ContentTypePropertiesCacheKey = "ContentType_PropertyTypes_Content:";
        
        public const string IdToKeyCacheKey = "UI2K__";
        public const string KeyToIdCacheKey = "UK2I__";
    }
}
