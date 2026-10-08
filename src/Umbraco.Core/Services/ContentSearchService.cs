using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides backoffice child search for documents.
/// </summary>
public sealed class ContentSearchService : ContentSearchServiceBase<IContent>, IContentSearchService
{
    private readonly IContentService _contentService;

    public ContentSearchService(
        ISearcherResolver searcherResolver,
        IContentService contentService,
        ILogger<ContentSearchService> logger)
        : base(searcherResolver, logger)
        => _contentService = contentService;

    /// <inheritdoc />
    protected override UmbracoObjectTypes ObjectType => UmbracoObjectTypes.Document;

    /// <inheritdoc />
    protected override string IndexAlias => Constants.Search.IndexAliases.DraftContent;

    /// <inheritdoc />
    protected override async Task<PagedModel<IContent>> SearchChildrenFromDatabaseAsync(Guid? parentId, string[]? propertyAliases, Ordering? ordering, bool loadTemplates, int skip, int take)
        => loadTemplates
            ? await _contentService.GetChildrenAsync(parentId, skip, take, propertyAliases, ordering, CancellationToken.None)
            : await _contentService.GetChildrenWithoutTemplatesAsync(parentId, skip, take, propertyAliases, ordering, CancellationToken.None);

    /// <inheritdoc />
    protected override Task<IEnumerable<IContent>> GetItemsAsync(IEnumerable<Guid> keys, string[]? propertyAliases, bool loadTemplates, CancellationToken cancellationToken)
        => _contentService.GetByIdsAsync(keys, propertyAliases, loadTemplates, cancellationToken);
}
