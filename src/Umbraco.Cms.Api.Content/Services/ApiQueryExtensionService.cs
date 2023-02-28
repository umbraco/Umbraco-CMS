using Examine;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Content.Services;

public class ApiQueryExtensionService : IApiQueryExtensionService
{
    private readonly IExamineManager _examineManager;

    public ApiQueryExtensionService(IExamineManager examineManager) => _examineManager = examineManager;

    /// <inheritdoc/>
    public IEnumerable<Guid> GetGuidsFromResults(string fieldName, Guid id, Func<ISearchResults, IEnumerable<Guid>> processResults)
    {
        if (!_examineManager.TryGetIndex(Constants.UmbracoIndexes.ContentAPIIndexName, out IIndex? apiIndex))
        {
            return Enumerable.Empty<Guid>(); // Return attempt?
        }

        ISearcher searcher = apiIndex.Searcher;
        ISearchResults results = searcher
            .CreateQuery()
            .Field(fieldName, id.ToString())
            .Execute();

        return processResults(results);
    }
}
