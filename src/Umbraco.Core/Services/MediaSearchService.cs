using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides backoffice child search for media.
/// </summary>
public sealed class MediaSearchService : ContentSearchServiceBase<IMedia>, IMediaSearchService
{
    private readonly IMediaService _mediaService;

    public MediaSearchService(
        ISearcherResolver searcherResolver,
        IMediaService mediaService,
        ILogger<MediaSearchService> logger)
        : base(searcherResolver, logger)
        => _mediaService = mediaService;

    /// <inheritdoc />
    protected override UmbracoObjectTypes ObjectType => UmbracoObjectTypes.Media;

    /// <inheritdoc />
    protected override string IndexAlias => Constants.Search.IndexAliases.DraftMedia;

    /// <inheritdoc />
    protected override Task<PagedModel<IMedia>> SearchChildrenFromDatabaseAsync(Guid? parentId, string[]? propertyAliases, Ordering? ordering, bool loadTemplates, int skip, int take)
        => _mediaService.GetChildrenAsync(parentId, skip, take, propertyAliases, ordering, CancellationToken.None);

    /// <inheritdoc />
    protected override Task<IEnumerable<IMedia>> GetItemsAsync(IEnumerable<Guid> keys, string[]? propertyAliases, bool loadTemplates, CancellationToken cancellationToken)
        => _mediaService.GetByIdsAsync(keys, propertyAliases, cancellationToken);
}
