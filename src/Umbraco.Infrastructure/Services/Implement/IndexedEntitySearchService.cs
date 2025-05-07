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

    public IndexedEntitySearchService(IBackOfficeExamineSearcher backOfficeExamineSearcher, IEntityService entityService)
        : this(
            backOfficeExamineSearcher,
            entityService,
            StaticServiceProvider.Instance.GetRequiredService<IContentTypeService>(),
            StaticServiceProvider.Instance.GetRequiredService<IMediaTypeService>(),
            StaticServiceProvider.Instance.GetRequiredService<IMemberTypeService>())
    {
    }

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

    public PagedModel<IEntitySlim> Search(UmbracoObjectTypes objectType, string query, int skip = 0, int take = 100, bool ignoreUserStartNodes = false)
        => Search(objectType, query, null, skip, take, ignoreUserStartNodes);

    public PagedModel<IEntitySlim> Search(
        UmbracoObjectTypes objectType,
        string query,
        Guid? parentId,
        int skip = 0,
        int take = 100,
        bool ignoreUserStartNodes = false)
        => Search(objectType, query, parentId, null, null, skip, take, ignoreUserStartNodes);

    public PagedModel<IEntitySlim> Search(
        UmbracoObjectTypes objectType,
        string query,
        Guid? parentId,
        IEnumerable<Guid>? contentTypeIds,
        int skip = 0,
        int take = 100,
        bool ignoreUserStartNodes = false)
        => Search(objectType, query, parentId, contentTypeIds, null, skip, take, ignoreUserStartNodes);

    public PagedModel<IEntitySlim> Search(
        UmbracoObjectTypes objectType,
        string query,
        Guid? parentId,
        IEnumerable<Guid>? contentTypeIds,
        bool? trashed,
        int skip = 0,
        int take = 100,
        bool ignoreUserStartNodes = false)
        => SearchAsync(objectType, query, parentId, contentTypeIds, trashed, skip, take, ignoreUserStartNodes).GetAwaiter().GetResult();

    public Task<PagedModel<IEntitySlim>> SearchAsync(
        UmbracoObjectTypes objectType,
        string query,
        Guid? parentId,
        IEnumerable<Guid>? contentTypeIds,
        bool? trashed,
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
