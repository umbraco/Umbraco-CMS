using Umbraco.Cms.Api.Management.ViewModels.HealthCheck;
using Umbraco.Cms.Core.HealthChecks;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IHealthCheckGroupPresentationFactory
{
    IEnumerable<IGrouping<string?, HealthCheck>> CreateGroupingFromHealthCheckCollection();

    HealthCheckGroupWithResultResponseModel CreateHealthCheckGroupWithResultViewModel(IGrouping<string?, HealthCheck> healthCheckGroup);

    HealthCheckWithResultPresentationModel CreateHealthCheckWithResultViewModel(HealthCheck healthCheck);
}
