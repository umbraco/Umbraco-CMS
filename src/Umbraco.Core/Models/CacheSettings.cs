using System.ComponentModel;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Core.Models;

[UmbracoOptions(Constants.Configuration.ConfigCache)]
public class CacheSettings
{
    internal const int StaticDocumentBreadthFirstSeedCount = 100;
    internal const int StaticMediaBreadthFirstSeedCount = 100;
    internal const int StaticDocumentSeedBatchSize = 100;
    internal const int StaticMediaSeedBatchSize = 100;

    /// <summary>
    /// Gets or sets a value for the collection of content type ids to always have in the cache.
    /// </summary>
    public List<Guid> ContentTypeKeys { get; set; } =
        new();

    /// <summary>
    /// Gets or sets a value for the document breadth first seed count.
    /// </summary>
    [DefaultValue(StaticDocumentBreadthFirstSeedCount)]
    public int DocumentBreadthFirstSeedCount { get; set; } = StaticDocumentBreadthFirstSeedCount;

    /// <summary>
    /// Gets or sets a value for the media breadth first seed count.
    /// </summary>
    [DefaultValue(StaticMediaBreadthFirstSeedCount)]
    public int MediaBreadthFirstSeedCount { get; set; } = StaticDocumentBreadthFirstSeedCount;

    /// <summary>
    /// Gets or sets a value for the document seed batch size.
    /// </summary>
    [DefaultValue(StaticDocumentSeedBatchSize)]
    public int DocumentSeedBatchSize { get; set; } = StaticDocumentSeedBatchSize;

    /// <summary>
    /// Gets or sets a value for the media seed batch size.
    /// </summary>
    [DefaultValue(StaticMediaSeedBatchSize)]
    public int MediaSeedBatchSize { get; set; } = StaticMediaSeedBatchSize;

    public CacheEntry Entry { get; set; } = new CacheEntry();

    public class CacheEntry
    {
        public CacheEntrySettings Document { get; set; } = new CacheEntrySettings();

        public CacheEntrySettings Media { get; set; } = new CacheEntrySettings();
    }
}
