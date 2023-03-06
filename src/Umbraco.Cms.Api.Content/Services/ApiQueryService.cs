using Examine;
using Examine.Search;
using Umbraco.Cms.Api.Content.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.ContentApi;

namespace Umbraco.Cms.Api.Content.Services;
public class ApiQueryService : IApiQueryService
{
    private readonly IApiQueryExtensionService _apiQueryExtensionService;
    private readonly List<IQueryOptionHandler> _queryOptionHandlers; // change to collect handlers from scope
    private readonly IExamineManager _examineManager;

    // ToDo: Create QueryOptionHandlerCollection
    //public ApiQueryService(IApiQueryExtensionService apiQueryExtensionService, List<IQueryOptionHandler> queryOptionHandlers, IExamineManager examineManager)
    public ApiQueryService(IApiQueryExtensionService apiQueryExtensionService, IExamineManager examineManager)
    {
        _apiQueryExtensionService = apiQueryExtensionService;
        _queryOptionHandlers = new List<IQueryOptionHandler> { new ChildrenQueryOption(), new DescendantsQueryOption() };
        _examineManager = examineManager;
    }

    /// <inheritdoc/>
    public ApiQueryType GetQueryType(string queryOption)
    {
        if (queryOption.StartsWith("ancestors:", StringComparison.OrdinalIgnoreCase))
        {
            return ApiQueryType.Ancestors;
        }
        else
        {
            return ApiQueryType.Unknown;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<Guid> GetGuidsFromQuery(Guid id, ApiQueryType queryType)
    {
        switch (queryType)
        {
            case ApiQueryType.Ancestors:
                return GetAncestorIds(id);
            default:
                return Enumerable.Empty<Guid>(); // throw new ArgumentOutOfRangeException("Invalid query type");
        }
    }

    public IEnumerable<Guid> ExecuteQuery(Dictionary<string, string> queryParams, string fieldValue)
    {
        IQueryOptionHandler? queryHandler = _queryOptionHandlers.FirstOrDefault(h => h.CanHandle(queryParams["fetch"]));

        if (queryHandler is null)
        {
            return Enumerable.Empty<Guid>();
        }

        if (!_examineManager.TryGetIndex(Constants.UmbracoIndexes.ContentAPIIndexName, out IIndex? apiIndex))
        {
            return Enumerable.Empty<Guid>();
        }

        IQuery baseQuery = apiIndex.Searcher.CreateQuery();
        IBooleanOperation queryOperation = queryHandler
            .BuildApiIndexQuery(baseQuery, fieldValue);

        if (queryParams.ContainsKey("filter"))
        {
            var alias = GetContentTypeAliasFromFilter(queryParams["filter"]);

            if (alias is not null)
            {
                queryOperation = queryOperation
                    .And()
                    .Field("__NodeTypeAlias", alias);
            }
        }

        ISearchResults results = queryOperation.Execute();

        return results.Select(x => Guid.Parse(x.Id));
    }

    private IEnumerable<Guid> GetAncestorIds(Guid id)
        => _apiQueryExtensionService.GetGuidsFromResults("id", id, results =>
        {
            var stringGuids = results.FirstOrDefault()?.Values.GetValueOrDefault("ancestorKeys");
            var guids = new List<Guid>();
            if (!string.IsNullOrEmpty(stringGuids))
            {
                guids = stringGuids.Split(',').Select(s => Guid.Parse(s)).ToList();
            }

            return guids;
        });

    private string? GetContentTypeAliasFromFilter(string filterValue)
    {
        if (!filterValue.StartsWith("contentType", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return filterValue.Substring(filterValue.IndexOf(':', StringComparison.Ordinal) + 1);
    }
}
