using Examine;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

internal sealed class IndexedEntitySearchService : IIndexedEntitySearchService
{
    private readonly IBackOfficeExamineSearcher _backOfficeExamineSearcher;
    private readonly IEntityService _entityService;
    private readonly IContentTypeService _contentTypeService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IMemberTypeService _memberTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexedEntitySearchService"/> class.
    /// </summary>
    /// <param name="backOfficeExamineSearcher">The searcher used to query the back office Examine index.</param>
    /// <param name="entityService">The service used to retrieve and manage Umbraco entities.</param>
    public IndexedEntitySearchService(IBackOfficeExamineSearcher backOfficeExamineSearcher, IEntityService entityService)
        : this(
            backOfficeExamineSearcher,
            entityService,
            StaticServiceProvider.Instance.GetRequiredService<IContentTypeService>(),
            StaticServiceProvider.Instance.GetRequiredService<IMediaTypeService>(),
            StaticServiceProvider.Instance.GetRequiredService<IMemberTypeService>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexedEntitySearchService"/> class, which provides search functionality for indexed entities in the back office.
    /// </summary>
    /// <param name="backOfficeExamineSearcher">The searcher used to query the back office Examine index.</param>
    /// <param name="entityService">Service for managing and retrieving entities.</param>
    /// <param name="contentTypeService">Service for managing content types.</param>
    /// <param name="mediaTypeService">Service for managing media types.</param>
    /// <param name="memberTypeService">Service for managing member types.</param>
    public IndexedEntitySearchService(
        IBackOfficeExamineSearcher backOfficeExamineSearcher,
        IEntityService entityService,
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IMemberTypeService memberTypeService)
    {
        _backOfficeExamineSearcher = backOfficeExamineSearcher;
        _entityService = entityService;
        _contentTypeService = contentTypeService;
        _mediaTypeService = mediaTypeService;
        _memberTypeService = memberTypeService;
    }

    /// <summary>
    /// Asynchronously searches for entities of the specified Umbraco object type using the provided query and optional filters.
    /// </summary>
    /// <param name="objectType">The type of Umbraco object to search for (Document, Media, or Member).</param>
    /// <param name="query">The search query string to use for matching entities.</param>
    /// <param name="parentId">An optional parent entity ID to restrict the search to descendants of a specific parent.</param>
    /// <param name="contentTypeIds">An optional collection of content type IDs to filter the search results by specific content types.</param>
    /// <param name="trashed">An optional flag indicating whether to include only trashed, only non-trashed, or all entities.</param>
    /// <param name="culture">An optional culture code to filter results by culture-specific content.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for paging).</param>
    /// <param name="take">The maximum number of items to return in the result set (used for paging).</param>
    /// <param name="ignoreUserStartNodes">If true, ignores user start nodes when performing the search.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a <see cref="PagedModel{IEntitySlim}"/> with the matching entities and the total result count.
    /// </returns>
    public Task<PagedModel<IEntitySlim>> SearchAsync(
        UmbracoObjectTypes objectType,
        string query,
        Guid? parentId,
        IEnumerable<Guid>? contentTypeIds,
        bool? trashed,
        string? culture = null,
        int skip = 0,
        int take = 100,
        bool ignoreUserStartNodes = false)
    {
        UmbracoEntityTypes entityType = objectType switch
        {
            UmbracoObjectTypes.Document => UmbracoEntityTypes.Document,
            UmbracoObjectTypes.Media => UmbracoEntityTypes.Media,
            UmbracoObjectTypes.Member => UmbracoEntityTypes.Member,
            _ => throw new NotSupportedException("This service only supports searching for documents, media and members")
        };

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);

        Guid[]? contentTypeIdsAsArray = contentTypeIds as Guid[] ?? contentTypeIds?.ToArray();
        var contentTypeAliases = contentTypeIdsAsArray?.Length > 0
            ? (entityType switch
            {
                UmbracoEntityTypes.Document => _contentTypeService.GetMany(contentTypeIdsAsArray).Select(x => x.Alias),
                UmbracoEntityTypes.Media => _mediaTypeService.GetMany(contentTypeIdsAsArray).Select(x => x.Alias),
                UmbracoEntityTypes.Member => _memberTypeService.GetMany(contentTypeIdsAsArray).Select(x => x.Alias),
                _ => throw new NotSupportedException("This service only supports searching for documents, media and members")
            }).ToArray()
            : null;

        IEnumerable<ISearchResult> searchResults = _backOfficeExamineSearcher.Search(
            query,
            entityType,
            pageSize,
            pageNumber,
            out var totalFound,
            contentTypeAliases,
            trashed,
            ignoreUserStartNodes: ignoreUserStartNodes,
            searchFrom: parentId?.ToString());

        Guid[] keys = searchResults.Select(
                result =>
                    result.Values.TryGetValue(UmbracoExamineFieldNames.NodeKeyFieldName, out var keyValue) &&
                    Guid.TryParse(keyValue, out Guid key)
                        ? key
                        : Guid.Empty)
            .Where(key => key != Guid.Empty)
            .ToArray();

        return Task.FromResult(new PagedModel<IEntitySlim>
        {
            Items = keys.Any()
                ? _entityService.GetAll(objectType, keys)
                : Enumerable.Empty<IEntitySlim>(),
            Total = totalFound
        });
    }
}
