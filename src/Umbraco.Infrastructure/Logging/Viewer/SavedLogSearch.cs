namespace Umbraco.Cms.Core.Logging.Viewer;

/// <summary>
/// Represents a saved log search, including the criteria and metadata used to retrieve specific log entries.
/// </summary>
public class SavedLogSearch
{
    /// <summary>
    /// The name of the saved log search.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the query used in the saved log search.
    /// </summary>
    public required string Query { get; set; }
}
