namespace Umbraco.Cms.Core.Logging.Viewer;

/// <summary>
/// Represents the configuration options available for customizing the behavior of the log viewer in Umbraco.
/// </summary>
[Obsolete("Use ILogViewerService instead. Scheduled for removal in Umbraco 15.")]
public interface ILogViewerConfig
{
    /// <summary>
    /// Retrieves the collection of saved log searches configured in the log viewer.
    /// </summary>
    /// <returns>A read-only list containing the saved log searches.</returns>
    IReadOnlyList<SavedLogSearch> GetSavedSearches();

    /// <summary>Adds a saved search with the specified name and query.</summary>
    /// <param name="name">The name of the saved search.</param>
    /// <param name="query">The query string for the saved search.</param>
    /// <returns>A read-only list of saved log searches including the newly added one.</returns>
    IReadOnlyList<SavedLogSearch> AddSavedSearch(string name, string query);

    /// <summary>Deletes the saved log search with the specified name and returns the updated list of saved searches.</summary>
    /// <param name="name">The name of the saved search to delete.</param>
    /// <returns>A read-only list of the remaining saved log searches.</returns>
    IReadOnlyList<SavedLogSearch> DeleteSavedSearch(string name);
}
