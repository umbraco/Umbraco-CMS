using Umbraco.Cms.Api.Management.ViewModels.HealthCheck;
using Umbraco.Cms.Core.HealthChecks;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IHealthCheckGroupViewModelFactory
{
    IEnumerable<IGrouping<string?, HealthCheck>> CreateGroupingFromHealthCheckCollection(HealthCheckCollection healthChecks);

    HealthCheckGroupWithResultViewModel CreateHealthCheckGroupWithResultViewModel(IGrouping<string?, HealthCheck> healthCheckGroup);

    HealthCheckWithResultViewModel CreateHealthCheckWithResultViewModel(HealthCheck healthCheck);
}
