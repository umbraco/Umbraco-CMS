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

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentSearchService"/> class.
    /// </summary>
    /// <param name="searcherResolver">The resolver used to obtain the searcher for the content index.</param>
    /// <param name="contentService">The service used to retrieve content items and their children from the database.</param>
    /// <param name="idKeyMap">The map used to resolve between content IDs and keys.</param>
    /// <param name="logger">The logger used to record warnings when a parent key cannot be resolved.</param>
    public ContentSearchService(
        ISearcherResolver searcherResolver,
        IContentService contentService,
        IIdKeyMap idKeyMap,
        ILogger<ContentSearchService> logger)
        : base(idKeyMap, searcherResolver, logger)
        => _contentService = contentService;

    /// <inheritdoc />
    protected override UmbracoObjectTypes ObjectType => UmbracoObjectTypes.Document;

    /// <inheritdoc />
    protected override string IndexAlias => Constants.Search.IndexAliases.DraftContent;

    /// <inheritdoc />
    protected override IEnumerable<IContent> SearchChildrenFromDatabase(int parentId, string[]? propertyAliases, Ordering? ordering, long pageNumber, int pageSize, bool loadTemplates, out long total)
        => _contentService.GetPagedChildren(parentId, pageNumber, pageSize, out total, propertyAliases, filter: null, ordering, loadTemplates);

    /// <inheritdoc />
    protected override IEnumerable<IContent> GetItems(IEnumerable<Guid> keys, string[]? propertyAliases, bool loadTemplates)
        => _contentService.GetByIds(keys, propertyAliases, loadTemplates);
}
