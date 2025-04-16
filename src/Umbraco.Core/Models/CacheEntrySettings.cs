using System.ComponentModel;

namespace Umbraco.Cms.Core.Models;

public class CacheEntrySettings
{
    internal const string StaticLocalCacheDuration = "1.00:00:00";
    internal const string StaticRemoteCacheDuration = "365.00:00:00";
    internal const string StaticSeedCacheDuration = "365.00:00:00";

    [DefaultValue(StaticLocalCacheDuration)]
    public  TimeSpan LocalCacheDuration { get; set; } = TimeSpan.Parse(StaticLocalCacheDuration);

    [DefaultValue(StaticRemoteCacheDuration)]
    public  TimeSpan RemoteCacheDuration { get; set; } = TimeSpan.Parse(StaticRemoteCacheDuration);

    [DefaultValue(StaticSeedCacheDuration)]
    public  TimeSpan SeedCacheDuration { get; set; } = TimeSpan.Parse(StaticSeedCacheDuration);

}
