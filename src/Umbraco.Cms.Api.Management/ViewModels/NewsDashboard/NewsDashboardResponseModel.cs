namespace Umbraco.Cms.Api.Management.ViewModels.NewsDashboard;

/// <summary>
/// Represents the data returned by the News Dashboard API endpoint.
/// </summary>
public class NewsDashboardResponseModel
{
    /// <summary>
    /// Gets or sets the collection of items displayed on the news dashboard.
    /// </summary>
    public required IEnumerable<NewsDashboardItemResponseModel> Items { get; set; }
}
