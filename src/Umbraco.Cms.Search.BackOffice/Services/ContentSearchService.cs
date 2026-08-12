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
    public ContentSearchService(
        ISearcher searcher,
        IContentService contentService,
        IIdKeyMap idKeyMap,
        ILogger<ContentSearchService> logger)
        : base(idKeyMap, searcher, logger)
        => _contentService = contentService;

    /// <inheritdoc />
    protected override UmbracoObjectTypes ObjectType => UmbracoObjectTypes.Document;

    /// <inheritdoc />
    protected override string IndexAlias => Umbraco.Cms.Core.Constants.IndexAliases.DraftContent;

    /// <inheritdoc />
    protected override IEnumerable<IContent> SearchChildrenFromDatabase(int parentId, Ordering? ordering, long pageNumber, int pageSize, out long total)
        => _contentService.GetPagedChildren(parentId, pageNumber, pageSize, out total, null, ordering);

    /// <inheritdoc />
    protected override IEnumerable<IContent> GetItems(IEnumerable<Guid> keys)
        => _contentService.GetByIds(keys);
}
