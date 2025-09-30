namespace Umbraco.Cms.Core;
public static partial class Constants
{
    public static class Cache
    {
        public static class Tags
        {
            public const string Content = "content";

            public const string Media = "media";
        }

        /// <summary>
        /// Defines the string used to represent a null value in the cache.
        /// </summary>
        /// <remarks>
        /// Used in conjunction with the option to cache null values on the repository caches, so we
        /// can distinguish a true null "not found" value and a cached null value.</remarks>
        public const string NullRepresentationInCache = "*NULL*";
    }
}
