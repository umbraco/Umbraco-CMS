namespace Umbraco.Cms.Api.Management.ViewModels.NewsDashboard;

public class NewsDashboardResponseModel
{
    public required IEnumerable<NewsDashboardArticleResponseModel> Articles { get; set; }
}
