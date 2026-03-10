namespace Umbraco.Cms.Api.Management.ViewModels.NewsDashboard;

public class NewsDashboardItemResponseModel
{
    public required string Priority { get; set; }

    public required string Header { get; set; }

    public string? Body { get; set; }

    public string? ButtonText { get; set; }

    public string? ImageUrl { get; set; }

    public string? ImageAltText { get; set; }

    public string? Url { get; set; }
}
