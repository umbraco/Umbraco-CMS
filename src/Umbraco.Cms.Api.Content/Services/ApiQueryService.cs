using Umbraco.Cms.Core.ContentApi;

namespace Umbraco.Cms.Api.Content.Services;
public class ApiQueryService : IApiQueryService
{
    private readonly IApiQueryExtensionService _apiQueryExtensionService;

    public ApiQueryService(IApiQueryExtensionService apiQueryExtensionService) => _apiQueryExtensionService = apiQueryExtensionService;

    /// <inheritdoc/>
    public ApiQueryType GetQueryType(string queryOption)
    {
        if (queryOption.StartsWith("children:", StringComparison.OrdinalIgnoreCase))
        {
            return ApiQueryType.Children;
        }
        else if (queryOption.StartsWith("descendants:", StringComparison.OrdinalIgnoreCase))
        {
            return ApiQueryType.Descendants;
        }
        else if (queryOption.StartsWith("ancestors:", StringComparison.OrdinalIgnoreCase))
        {
            return ApiQueryType.Ancestors;
        }
        else
        {
            return ApiQueryType.Unknown;
        }
    }

    /// <inheritdoc/>
    public Guid? GetGuidFromFetch(string fetchQuery)
    {
        var guidString = fetchQuery.Substring(fetchQuery.IndexOf(':', StringComparison.Ordinal) + 1);

        if (Guid.TryParse(guidString, out Guid id))
        {
            return id;
        }

        return null;
    }

    /// <inheritdoc/>
    public IEnumerable<Guid> GetGuidsFromQuery(Guid id, ApiQueryType queryType)
    {
        switch (queryType)
        {
            case ApiQueryType.Children:
                return GetChildrenIds(id);
            case ApiQueryType.Descendants:
                return GetDescendantIds(id);
            case ApiQueryType.Ancestors:
                return GetAncestorIds(id);
            default:
                return Enumerable.Empty<Guid>(); // throw new ArgumentOutOfRangeException("Invalid query type");
        }
    }

    private IEnumerable<Guid> GetChildrenIds(Guid id)
        => _apiQueryExtensionService.GetGuidsFromResults("parentKey", id, results => results.Select(x => Guid.Parse(x.Id)));

    private IEnumerable<Guid> GetDescendantIds(Guid id)
        => _apiQueryExtensionService.GetGuidsFromResults("ancestorKeys", id, results => results.Select(x => Guid.Parse(x.Id)));

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
}
