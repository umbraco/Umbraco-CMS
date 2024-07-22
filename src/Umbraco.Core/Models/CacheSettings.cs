using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Core.Models;

[UmbracoOptions(Constants.Configuration.ConfigCache)]
public class CacheSettings
{
    /// <summary>
    ///     Gets or sets a value for the collection of content type ids to always have in the cache.
    /// </summary>
    public List<int> ContentTypeIds { get; set; } =
        new List<int>();
}
