using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Scoping;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Core.Logging.Viewer;

/// <summary>
/// Represents the configuration settings used by the Log Viewer in Umbraco.
/// </summary>
public class LogViewerConfig : ILogViewerConfig
{
    private readonly ILogViewerQueryRepository _logViewerQueryRepository;
    private readonly IScopeProvider _scopeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Core.Logging.Viewer.LogViewerConfig"/> class.
    /// </summary>
    /// <param name="logViewerQueryRepository">An instance used to query log data for the log viewer.</param>
    /// <param name="scopeProvider">The provider that manages database transaction scopes.</param>
    public LogViewerConfig(ILogViewerQueryRepository logViewerQueryRepository, IScopeProvider scopeProvider)
    {
        _logViewerQueryRepository = logViewerQueryRepository;
        _scopeProvider = scopeProvider;
    }

    /// <summary>
    /// Gets the saved log searches.
    /// </summary>
    /// <returns>A read-only list of saved log searches.</returns>
    /// <remarks>
    /// This method is obsolete. Use <see cref="ILogViewerService.GetSavedLogQueriesAsync"/> instead.
    /// </remarks>
    [Obsolete("Use ILogViewerService.GetSavedLogQueriesAsync instead. Scheduled for removal in Umbraco 15.")]
    public IReadOnlyList<SavedLogSearch> GetSavedSearches()
    {
        using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
        IEnumerable<ILogViewerQuery> logViewerQueries = _logViewerQueryRepository.GetMany();
        SavedLogSearch[] result = logViewerQueries.Select(x => new SavedLogSearch() { Name = x.Name, Query = x.Query }).ToArray();
        return result;
    }

    /// <summary>
    /// Adds a saved search with the specified name and query.
    /// </summary>
    /// <param name="name">The name of the saved search.</param>
    /// <param name="query">The query string for the saved search.</param>
    /// <returns>A read-only list of all saved log searches.</returns>
    /// <remarks>
    /// This method is obsolete. Use <see cref="ILogViewerService.AddSavedLogQueryAsync"/> instead. Scheduled for removal in Umbraco 15.
    /// </remarks>
    [Obsolete("Use ILogViewerService.AddSavedLogQueryAsync instead. Scheduled for removal in Umbraco 15.")]
    public IReadOnlyList<SavedLogSearch> AddSavedSearch(string name, string query)
    {
        using IScope scope = _scopeProvider.CreateScope();
        _logViewerQueryRepository.Save(new LogViewerQuery(name, query));

        scope.Complete();
        return GetSavedSearches();
    }

    /// <summary>
    /// Deletes a saved log search with the specified name and returns the updated list of saved log searches.
    /// </summary>
    /// <param name="name">The name of the saved search to delete.</param>
    /// <returns>A read-only list containing the remaining saved log searches after the specified search has been deleted.</returns>
    /// <remarks>
    /// This method is obsolete. Use <see cref="ILogViewerService.DeleteSavedLogQueryAsync"/> instead. Scheduled for removal in Umbraco 15.
    /// If no saved search with the specified name exists, the method returns the current list of saved searches unchanged.
    /// </remarks>
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
