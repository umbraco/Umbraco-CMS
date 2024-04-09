namespace Umbraco.Cms.Core.Logging.Viewer;

[Obsolete("Use ILogViewerService instead. Scheduled for removal in Umbraco 15.")]
public interface ILogViewerConfig
{
    IReadOnlyList<SavedLogSearch> GetSavedSearches();

    IReadOnlyList<SavedLogSearch> AddSavedSearch(string name, string query);

    IReadOnlyList<SavedLogSearch> DeleteSavedSearch(string name);
}
