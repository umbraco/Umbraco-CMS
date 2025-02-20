using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Scoping;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Core.Logging.Viewer;

public class LogViewerConfig : ILogViewerConfig
{
    private readonly ILogViewerQueryRepository _logViewerQueryRepository;
    private readonly IScopeProvider _scopeProvider;

    public LogViewerConfig(ILogViewerQueryRepository logViewerQueryRepository, IScopeProvider scopeProvider)
    {
        _logViewerQueryRepository = logViewerQueryRepository;
        _scopeProvider = scopeProvider;
    }

    [Obsolete("Use ILogViewerService.GetSavedLogQueriesAsync instead. Scheduled for removal in Umbraco 15.")]
    public IReadOnlyList<SavedLogSearch> GetSavedSearches()
    {
        using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
        IEnumerable<ILogViewerQuery> logViewerQueries = _logViewerQueryRepository.GetMany();
        SavedLogSearch[] result = logViewerQueries.Select(x => new SavedLogSearch() { Name = x.Name, Query = x.Query }).ToArray();
        return result;
    }

    [Obsolete("Use ILogViewerService.AddSavedLogQueryAsync instead. Scheduled for removal in Umbraco 15.")]
    public IReadOnlyList<SavedLogSearch> AddSavedSearch(string name, string query)
    {
        using IScope scope = _scopeProvider.CreateScope();
        _logViewerQueryRepository.Save(new LogViewerQuery(name, query));

        scope.Complete();
        return GetSavedSearches();
    }

    [Obsolete("Use ILogViewerService.DeleteSavedLogQueryAsync instead. Scheduled for removal in Umbraco 15.")]
    public IReadOnlyList<SavedLogSearch> DeleteSavedSearch(string name)
    {
        using IScope scope = _scopeProvider.CreateScope();
        ILogViewerQuery? item = _logViewerQueryRepository.GetByName(name);
        if (item is not null)
        {
            _logViewerQueryRepository.Delete(item);
        }

        // Return the updated object - so we can instantly reset the entire array from the API response
        IReadOnlyList<SavedLogSearch> result =  GetSavedSearches()!;
        scope.Complete();
        return result;
    }
}
