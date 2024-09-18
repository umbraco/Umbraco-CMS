using System.ComponentModel;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Core.Models;

[UmbracoOptions(Constants.Configuration.ConfigCache)]
public class CacheSettings
{
    internal const int StaticBreadthFirstSeedCount = 100;

    /// <summary>
    ///     Gets or sets a value for the collection of content type ids to always have in the cache.
    /// </summary>
    public List<Guid> ContentTypeKeys { get; set; } =
        new();

    [DefaultValue(StaticBreadthFirstSeedCount)]
    public int BreadthFirstSeedCount { get; set; } = StaticBreadthFirstSeedCount;

    public TimeSpan SeedCacheDuration { get; set; } = TimeSpan.FromDays(365);
}
