namespace Umbraco.Cms.Api.Management.ViewModels.LogViewer;

/// <summary>
/// Serves as the base class for representing saved log search presentations in the Log Viewer.
/// </summary>
public class SavedLogSearchPresenationBase
{
    /// <summary>
    ///     Gets or sets the name of the saved search.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     Gets or sets the query of the saved search.
    /// </summary>
    public required string Query { get; set; }
}
