using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Core.Models.Searching.Sorting;
using Umbraco.Cms.Search.Core.Services;
using Constants = Umbraco.Cms.Search.Core.Constants;

namespace Umbraco.Cms.Search.BackOffice.Services;

internal sealed class IndexedEntitySearchService : IndexedSearchServiceBase, IIndexedEntitySearchService
{
    private readonly ISearcherResolver _searcherResolver;
    private readonly IEntityService _entityService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly AppCaches _appCaches;
    private readonly IIdKeyMap _idKeyMap;

    public IndexedEntitySearchService(ISearcherResolver searcherResolver, IEntityService entityService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor, AppCaches appCaches, IIdKeyMap idKeyMap)
    {
        _searcherResolver = searcherResolver;
        _entityService = entityService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _appCaches = appCaches;
        _idKeyMap = idKeyMap;
    }

    public PagedModel<IEntitySlim> Search(UmbracoObjectTypes objectType, string query, int skip = 0, int take = 100, bool ignoreUserStartNodes = false)
        => SearchAsync(objectType, query, null, null, null, null, skip, take, ignoreUserStartNodes).GetAwaiter().GetResult();

    public async Task<PagedModel<IEntitySlim>> SearchAsync(
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
        Guid[] startNodeKeys = CurrentUserStartNodeKeys(objectType);

        // cannot combine trashed and start node filtering - this should always yield zero results
        if (startNodeKeys.Length > 0 && trashed is true)
        {
            return new PagedModel<IEntitySlim>();
        }

        var indexAlias = objectType switch
        {
            UmbracoObjectTypes.Document => Constants.IndexAliases.DraftContent,
            UmbracoObjectTypes.Media => Constants.IndexAliases.DraftMedia,
            UmbracoObjectTypes.Member => Constants.IndexAliases.DraftMembers,
            _ => throw new ArgumentOutOfRangeException(nameof(objectType), objectType, null)
        };

        List<Filter> filters = ParseFilters(query, parentId, out var effectiveQuery);

        Guid[]? contentTypeIdsAsArray = contentTypeIds as Guid[] ?? contentTypeIds?.ToArray();
        if (contentTypeIdsAsArray?.Length > 0)
        {
            filters.Add(
                new KeywordFilter(
                    FieldName: Constants.FieldNames.ContentTypeId,
                    Values: contentTypeIdsAsArray.Select(contentTypeId => contentTypeId.AsKeyword()).ToArray(),
                    Negate: false));
        }

        if (startNodeKeys.Length > 0)
        {
            filters.Add(
                new KeywordFilter(
                    FieldName: Constants.FieldNames.PathIds,
                    Values: startNodeKeys.Select(key => key.AsKeyword()).ToArray(),
                    Negate: false));
        }
        else if (trashed.HasValue)
        {
            Guid? recycleBinId = objectType switch
            {
                UmbracoObjectTypes.Document => Cms.Core.Constants.System.RecycleBinContentKey,
                UmbracoObjectTypes.Media => Cms.Core.Constants.System.RecycleBinMediaKey,
                _ => null
            };

            if (recycleBinId.HasValue)
            {
                filters.Add(
                    new KeywordFilter(
                        FieldName: Constants.FieldNames.PathIds,
                        Values: [recycleBinId.Value.AsKeyword()],
                        Negate: trashed.Value is false));
            }
        }

        ISearcher searcher = _searcherResolver.GetRequiredSearcher(indexAlias);
        SearchResult result = await searcher.SearchAsync(
            indexAlias,
            query: effectiveQuery,
            filters: filters,
            facets: null,
            sorters: [Sorting.Default()],
            culture: culture,
            segment: null,
            accessContext: null,
            skip,
            take);

        Guid[] resultKeys = result.Documents.Select(d => d.Id).ToArray();
        IEntitySlim[] resultEntities = resultKeys.Any()
            ? _entityService
                .GetAll(objectType, resultKeys)
                // unfortunately we can't explicitly rely on the underlying services ordering the requested
                // items correctly, so we need to enforce correct ordering here.
                .OrderBy(entity => resultKeys.IndexOf(entity.Key))
                .ToArray()
            : [];

        return new PagedModel<IEntitySlim> { Items = resultEntities, Total = result.Total };
    }

    private Guid[] CurrentUserStartNodeKeys(UmbracoObjectTypes objectType)
    {
        IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        var startNodeIds = objectType switch
        {
            UmbracoObjectTypes.Document => currentUser?.CalculateContentStartNodeIds(_entityService, _appCaches),
            UmbracoObjectTypes.Media => currentUser?.CalculateMediaStartNodeIds(_entityService, _appCaches),
            _ => null
        };

        return startNodeIds is not null
            ? startNodeIds.Select(id =>
                {
                    Attempt<Guid> attempt = _idKeyMap.GetKeyForId(id, objectType);
                    return attempt.Success ? attempt.Result : (Guid?)null;
                })
                .Where(key => key.HasValue)
                .Select(key => key!.Value)
                .ToArray()
            : [];
    }
}
