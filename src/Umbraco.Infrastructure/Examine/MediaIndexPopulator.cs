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

    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    public MediaIndexPopulator(ILogger<MediaIndexPopulator> logger, IMediaService mediaService, IValueSetBuilder<IMedia> mediaValueSetBuilder)
        : this(logger, null, mediaService, mediaValueSetBuilder, StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<IndexingSettings>>())
    {
    }

    /// <summary>
    ///     Default constructor to lookup all content data
    /// </summary>
    public MediaIndexPopulator(ILogger<MediaIndexPopulator> logger, IMediaService mediaService, IValueSetBuilder<IMedia> mediaValueSetBuilder, IOptionsMonitor<IndexingSettings> indexingSettings)
        : this(logger, null, mediaService, mediaValueSetBuilder, indexingSettings)
    {
    }

    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    public MediaIndexPopulator(ILogger<MediaIndexPopulator> logger, int? parentId, IMediaService mediaService, IValueSetBuilder<IMedia> mediaValueSetBuilder)
        : this(logger, parentId, mediaService, mediaValueSetBuilder, StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<IndexingSettings>>())
    {
    }

    /// <summary>
    ///     Optional constructor allowing specifying custom query parameters
    /// </summary>
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
