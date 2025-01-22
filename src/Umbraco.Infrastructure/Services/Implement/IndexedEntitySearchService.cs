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

    public IndexedEntitySearchService(IBackOfficeExamineSearcher backOfficeExamineSearcher, IEntityService entityService)
        : this(backOfficeExamineSearcher, entityService, StaticServiceProvider.Instance.GetRequiredService<IContentTypeService>())
    {
    }

    public IndexedEntitySearchService(
        IBackOfficeExamineSearcher backOfficeExamineSearcher,
        IEntityService entityService,
        IContentTypeService contentTypeService)
    {
        _backOfficeExamineSearcher = backOfficeExamineSearcher;
        _entityService = entityService;
        _contentTypeService = contentTypeService;
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
        => Search(objectType, query, parentId, null, skip, take, ignoreUserStartNodes);

    public PagedModel<IEntitySlim> Search(
        UmbracoObjectTypes objectType,
        string query,
        Guid? parentId,
        IEnumerable<Guid>? contentTypeIds,
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
            ? _contentTypeService.GetMany(contentTypeIdsAsArray).Select(x => x.Alias).ToArray()
            : null;

        IEnumerable<ISearchResult> searchResults = _backOfficeExamineSearcher.Search(
            query,
            entityType,
            pageSize,
            pageNumber,
            out var totalFound,
            contentTypeAliases,
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

        return new PagedModel<IEntitySlim>
        {
            Items = keys.Any()
                ? _entityService.GetAll(objectType, keys)
                : Enumerable.Empty<IEntitySlim>(),
            Total = totalFound
        };
    }
}
