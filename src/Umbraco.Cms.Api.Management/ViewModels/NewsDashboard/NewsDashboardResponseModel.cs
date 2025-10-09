namespace Umbraco.Cms.Api.Management.ViewModels.NewsDashboard;

public class NewsDashboardResponseModel
{
    public required IEnumerable<NewsDashboardItemResponseModel> Items { get; set; }
}
