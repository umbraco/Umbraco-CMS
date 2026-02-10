using System.ComponentModel;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents configuration settings for the Umbraco content and media cache.
/// </summary>
/// <remarks>
///     This class configures cache seeding behavior and cache entry durations for
///     documents and media items. Cache seeding pre-populates the cache during
///     application startup for improved performance.
/// </remarks>
[UmbracoOptions(Constants.Configuration.ConfigCache)]
public class CacheSettings
{
    /// <summary>
    ///     The default number of documents to seed using breadth-first traversal.
    /// </summary>
    internal const int StaticDocumentBreadthFirstSeedCount = 100;

    /// <summary>
    ///     The default number of media items to seed using breadth-first traversal.
    /// </summary>
    internal const int StaticMediaBreadthFirstSeedCount = 100;

    /// <summary>
    ///     The default batch size for seeding documents.
    /// </summary>
    internal const int StaticDocumentSeedBatchSize = 100;

    /// <summary>
    ///     The default batch size for seeding media items.
    /// </summary>
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

    /// <summary>
    ///     Gets or sets the cache entry settings for documents and media.
    /// </summary>
    public CacheEntry Entry { get; set; } = new CacheEntry();

    /// <summary>
    ///     Represents cache entry settings for documents and media items.
    /// </summary>
    public class CacheEntry
    {
        /// <summary>
        ///     Gets or sets the cache entry settings for documents.
        /// </summary>
        public CacheEntrySettings Document { get; set; } = new CacheEntrySettings();

        /// <summary>
        ///     Gets or sets the cache entry settings for media items.
        /// </summary>
        public CacheEntrySettings Media { get; set; } = new CacheEntrySettings();
    }
}
