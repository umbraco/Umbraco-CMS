using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Services;
using Constants = Umbraco.Cms.Search.Core.Constants;

namespace Umbraco.Cms.Search.BackOffice.Services;

/// <summary>
/// Provides backoffice child search for documents.
/// </summary>
internal sealed class ContentSearchService : ContentSearchServiceBase<IContent>, IContentSearchService
{
    private readonly IContentService _contentService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentSearchService"/> class.
    /// </summary>
    /// <param name="searcher">The searcher used to query the content index.</param>
    /// <param name="contentService">The service used to retrieve content items and their children from the database.</param>
    /// <param name="logger">The logger used to record warnings when a parent key cannot be resolved.</param>
    public ContentSearchService(
        ISearcher searcher,
        IContentService contentService,
        ILogger<ContentSearchService> logger)
        : base(searcher, logger)
        => _contentService = contentService;

    /// <inheritdoc />
    protected override UmbracoObjectTypes ObjectType => UmbracoObjectTypes.Document;

    /// <inheritdoc />
    protected override string IndexAlias => Umbraco.Cms.Core.Constants.IndexAliases.DraftContent;

    /// <inheritdoc />
    protected override async Task<PagedModel<IContent>> SearchChildrenFromDatabaseAsync(Guid? parentId, Ordering? ordering, int skip, int take)
        => await _contentService.GetChildrenAsync(parentId, skip, take, propertyAliases: null, ordering, CancellationToken.None);

    /// <inheritdoc />
    protected override IEnumerable<IContent> GetItems(IEnumerable<Guid> keys)
        => _contentService.GetByIdsAsync(keys, CancellationToken.None).GetAwaiter().GetResult();
}
