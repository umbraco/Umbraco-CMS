using System.IO;

namespace Umbraco.Core.Configuration.Models
{
    public class ImagingCacheSettings
    {
        public int MaxBrowserCacheDays { get; set; } = 7;

        public int MaxCacheDays { get; set; } = 365;

        public uint CachedNameLength { get; set; } = 8;

        public string CacheFolder { get; set; } = Path.Combine("~", "Umbraco", "Cache");
    }
}
