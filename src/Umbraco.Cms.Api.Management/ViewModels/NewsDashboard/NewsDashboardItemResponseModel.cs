namespace Umbraco.Cms.Api.Management.ViewModels.NewsDashboard;

/// <summary>
/// Represents the data returned for a single news item displayed on the news dashboard.
/// </summary>
public class NewsDashboardItemResponseModel
{
    /// <summary>
    /// Gets or sets the priority level of the news dashboard item, which may determine its importance or display order.
    /// </summary>
    public required string Priority { get; set; }

    /// <summary>
    /// Gets or sets the title or header text of the news dashboard item.
    /// </summary>
    public required string Header { get; set; }

    /// <summary>
    /// Gets or sets the main content of the news dashboard item.
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Gets or sets the text displayed on the button.
    /// </summary>
    public string? ButtonText { get; set; }

    /// <summary>
    /// Gets or sets the URL of the image associated with the news dashboard item.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the alternative text for the image.
    /// </summary>
    public string? ImageAltText { get; set; }

    /// <summary>Gets or sets the URL associated with the news dashboard item.</summary>
    public string? Url { get; set; }
}
