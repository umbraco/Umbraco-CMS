using Umbraco.Cms.Api.Management.ViewModels.NewsDashboard;

namespace Umbraco.Cms.Api.Management.Services.NewsDashboard;

public interface INewsDashboardService
{
    bool TryMapModel(string json, out IEnumerable<NewsDashboardItem>? newsDashboardItem);
}
