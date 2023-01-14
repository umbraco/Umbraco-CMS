namespace Umbraco.Cms.Core.Logging.Viewer;

public interface ILogViewerConfig
{
    [Obsolete("Use ILogViewerService.GetSavedLogQueriesAsync instead. Scheduled for removal in Umbraco 15.")]
    IReadOnlyList<SavedLogSearch> GetSavedSearches();

    [Obsolete("Use ILogViewerService.AddSavedLogQueryAsync instead. Scheduled for removal in Umbraco 15.")]
    IReadOnlyList<SavedLogSearch> AddSavedSearch(string name, string query);

    [Obsolete("Use the overload that only takes a 'name' parameter instead. This will be removed in Umbraco 14.")]
    IReadOnlyList<SavedLogSearch> DeleteSavedSearch(string name, string query);

    [Obsolete("Use ILogViewerService.DeleteSavedLogQueryAsync instead. Scheduled for removal in Umbraco 15.")]
    IReadOnlyList<SavedLogSearch> DeleteSavedSearch(string name) => DeleteSavedSearch(name, string.Empty);

    [Obsolete("Use ILogViewerService.GetSavedLogQueryByNameAsync instead. Scheduled for removal in Umbraco 15.")]
    SavedLogSearch? GetSavedSearchByName(string name);
}
