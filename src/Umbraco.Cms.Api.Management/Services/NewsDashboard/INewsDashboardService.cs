using Umbraco.Cms.Api.Management.ViewModels.NewsDashboard;

namespace Umbraco.Cms.Api.Management.Services.NewsDashboard;

public interface INewsDashboardService
{
    Task<NewsDashboardResponseModel> GetArticlesAsync();
}
