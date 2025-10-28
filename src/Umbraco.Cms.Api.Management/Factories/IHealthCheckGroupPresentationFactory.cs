using Umbraco.Cms.Api.Management.ViewModels.HealthCheck;
using Umbraco.Cms.Core.HealthChecks;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IHealthCheckGroupPresentationFactory
{
    IEnumerable<IGrouping<string?, HealthCheck>> CreateGroupingFromHealthCheckCollection();

    Task<HealthCheckGroupWithResultResponseModel> CreateHealthCheckGroupWithResultViewModelAsync(
        IGrouping<string?, HealthCheck> healthCheckGroup);

    Task<HealthCheckWithResultPresentationModel> CreateHealthCheckWithResultViewModelAsync(HealthCheck healthCheck);
}
