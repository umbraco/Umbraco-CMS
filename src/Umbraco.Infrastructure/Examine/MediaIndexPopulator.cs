using Examine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Performs the data lookups required to rebuild a media index
/// </summary>
public class MediaIndexPopulator : IndexPopulator<IUmbracoContentIndex>
{
    private readonly ILogger<MediaIndexPopulator> _logger;
    private readonly IMediaService _mediaService;
    private readonly IValueSetBuilder<IMedia> _mediaValueSetBuilder;
    private readonly int? _parentId;

    private IndexingSettings _indexingSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaIndexPopulator"/> class, which is responsible for populating the media index in Examine.
    /// </summary>
    /// <param name="logger">The logger used to record diagnostic and operational information for the media index population process.</param>
    /// <param name="mediaService">The service used to access and manage media items in Umbraco.</param>
    /// <param name="mediaValueSetBuilder">The builder that constructs value sets from media items for indexing.</param>
    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    public MediaIndexPopulator(ILogger<MediaIndexPopulator> logger, IMediaService mediaService, IValueSetBuilder<IMedia> mediaValueSetBuilder)
        : this(logger, null, mediaService, mediaValueSetBuilder, StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<IndexingSettings>>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaIndexPopulator"/> class, used to index all media content data.
    /// </summary>
    /// <param name="logger">The logger instance used for logging operations.</param>
    /// <param name="mediaService">The media service used to access media items.</param>
    /// <param name="mediaValueSetBuilder">The value set builder for constructing indexable values from media items.</param>
    /// <param name="indexingSettings">The indexing settings configuration.</param>
    public MediaIndexPopulator(ILogger<MediaIndexPopulator> logger, IMediaService mediaService, IValueSetBuilder<IMedia> mediaValueSetBuilder, IOptionsMonitor<IndexingSettings> indexingSettings)
        : this(logger, null, mediaService, mediaValueSetBuilder, indexingSettings)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaIndexPopulator"/> class, which is responsible for populating the media index in Examine.
    /// </summary>
    /// <param name="logger">The logger used for logging operations within the populator.</param>
    /// <param name="parentId">An optional parent ID to filter which media items are indexed. If <c>null</c>, all media items are considered.</param>
    /// <param name="mediaService">The service used to access and manage media data.</param>
    /// <param name="mediaValueSetBuilder">The builder used to create value sets for media items to be indexed.</param>
    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    public MediaIndexPopulator(ILogger<MediaIndexPopulator> logger, int? parentId, IMediaService mediaService, IValueSetBuilder<IMedia> mediaValueSetBuilder)
        : this(logger, parentId, mediaService, mediaValueSetBuilder, StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<IndexingSettings>>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaIndexPopulator"/> class, allowing specification of custom query parameters for media indexing.
    /// </summary>
    /// <param name="logger">The logger used for logging operations within the media index populator.</param>
    /// <param name="parentId">An optional parent media item ID to filter which media items are indexed.</param>
    /// <param name="mediaService">The service used to access and manage media items.</param>
    /// <param name="mediaValueSetBuilder">Builds value sets for media items to be indexed.</param>
    /// <param name="indexingSettings">Monitors configuration settings related to indexing.</param>
    public MediaIndexPopulator(ILogger<MediaIndexPopulator> logger, int? parentId, IMediaService mediaService, IValueSetBuilder<IMedia> mediaValueSetBuilder, IOptionsMonitor<IndexingSettings> indexingSettings)
    {
        _logger = logger;
        _parentId = parentId;
        _mediaService = mediaService;
        _mediaValueSetBuilder = mediaValueSetBuilder;
        _indexingSettings = indexingSettings.CurrentValue;

        indexingSettings.OnChange(change =>
        {
            _indexingSettings = change;
        });
    }

    protected override void PopulateIndexes(IReadOnlyList<IIndex> indexes)
    {
        if (indexes.Count == 0)
        {
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                _logger.LogDebug(
                $"{nameof(PopulateIndexes)} called with no indexes to populate. Typically means no index is registered with this populator.");
            }
            return;
        }

        var pageIndex = 0;

        var mediaParentId = -1;

        if (_parentId.HasValue && _parentId.Value > 0)
        {
            mediaParentId = _parentId.Value;
        }

        IMedia[] media;

        do
        {
            media = _mediaService.GetPagedDescendants(mediaParentId, pageIndex, _indexingSettings.BatchSize, out _).ToArray();

            // ReSharper disable once PossibleMultipleEnumeration
            foreach (IIndex index in indexes)
            {
                index.IndexItems(_mediaValueSetBuilder.GetValueSets(media));
            }

            pageIndex++;
        }
        while (media.Length == _indexingSettings.BatchSize);
    }
}
