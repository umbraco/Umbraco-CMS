using Examine;
using Microsoft.Extensions.Logging;
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

    /// <summary>
    ///     Default constructor to lookup all content data
    /// </summary>
    public MediaIndexPopulator(ILogger<MediaIndexPopulator> logger, IMediaService mediaService, IValueSetBuilder<IMedia> mediaValueSetBuilder)
        : this(logger, null, mediaService, mediaValueSetBuilder)
    {
    }

    /// <summary>
    ///     Optional constructor allowing specifying custom query parameters
    /// </summary>
    public MediaIndexPopulator(ILogger<MediaIndexPopulator> logger, int? parentId, IMediaService mediaService, IValueSetBuilder<IMedia> mediaValueSetBuilder)
    {
        _logger = logger;
        _parentId = parentId;
        _mediaService = mediaService;
        _mediaValueSetBuilder = mediaValueSetBuilder;
    }

    protected override void PopulateIndexes(IReadOnlyList<IIndex> indexes)
    {
        if (indexes.Count == 0)
        {
            _logger.LogDebug(
                $"{nameof(PopulateIndexes)} called with no indexes to populate. Typically means no index is registered with this populator.");
            return;
        }

        const int pageSize = 10000;
        var pageIndex = 0;

        var mediaParentId = -1;

        if (_parentId.HasValue && _parentId.Value > 0)
        {
            mediaParentId = _parentId.Value;
        }

        IMedia[] media;

        do
        {
            media = _mediaService.GetPagedDescendants(mediaParentId, pageIndex, pageSize, out _).ToArray();

            if (media.Length > 0)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (IIndex index in indexes)
                {
                    index.IndexItems(_mediaValueSetBuilder.GetValueSets(media));
                }
            }

            pageIndex++;
        }
        while (media.Length == pageSize);
    }
}
