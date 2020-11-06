using System;
using System.IO;
using System.Threading;

namespace Umbraco.Core.Configuration.Models
{
    public class ImagingCacheSettings
    {
        public TimeSpan BrowserMaxAge { get; set; } =  TimeSpan.FromDays(7);

        public TimeSpan CacheMaxAge { get; set; } = TimeSpan.FromDays(365);

        public uint CachedNameLength { get; set; } = 8;

        public string CacheFolder { get; set; } = Path.Combine("..", "umbraco", "mediacache");
    }
}
