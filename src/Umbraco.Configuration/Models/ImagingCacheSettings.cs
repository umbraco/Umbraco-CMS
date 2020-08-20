using System.Text.Json.Serialization;

namespace Umbraco.Configuration.Models
{
    public class ImagingCacheSettings
    {
        public int MaxBrowserCacheDays { get; set; } = 7;

        public int MaxCacheDays { get; set; } = 365;

        public uint CachedNameLength { get; set; } = 7;

        [JsonPropertyName("Folder")]
        public string CacheFolder { get; set; } = "../App_Data/Cache";
    }
}
