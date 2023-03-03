using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Search.Indexing.Populators;

/// <summary>
///     Performs the data lookups required to rebuild a media index
/// </summary>
public class MediaIndexPopulator : IndexPopulator
{
    private readonly ILogger<MediaIndexPopulator> _logger;
    private readonly IMediaService _mediaService;
    private readonly ISearchProvider _provider;
    private readonly int? _parentId;

    /// <summary>
    ///     Default constructor to lookup all content data
    /// </summary>
    public MediaIndexPopulator(ILogger<MediaIndexPopulator> logger, IMediaService mediaService, ISearchProvider provider)
        : this(logger, null, mediaService, provider)
    {
    }
    public override bool IsRegistered(string index)
    {
        if (base.IsRegistered(index))
        {
            return true;
        }

        var indexer = _provider.GetIndex(index);
        if (!(indexer is IUmbracoIndex<IContent> casted))
        {
            return false;
        }

        return true;
    }
    /// <summary>
    ///     Optional constructor allowing specifying custom query parameters
    /// </summary>
    public MediaIndexPopulator(ILogger<MediaIndexPopulator> logger, int? parentId, IMediaService mediaService, ISearchProvider provider)
    {
        _logger = logger;
        _parentId = parentId;
        _mediaService = mediaService;
        _provider = provider;
    }

    protected override void PopulateIndexes(IReadOnlyList<string> indexes)
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

            // ReSharper disable once PossibleMultipleEnumeration
            foreach (string index in indexes)
            {
                _provider.GetIndex<IMedia>(index)?.IndexItems(media);
            }

            pageIndex++;
        }
        while (media.Length == pageSize);
    }
}
