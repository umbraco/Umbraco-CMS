using Umbraco.Cms.Api.Management.ViewModels.HealthCheck;
using Umbraco.Cms.Core.HealthChecks;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IHealthCheckGroupPresentationFactory
{
    IEnumerable<IGrouping<string?, HealthCheck>> CreateGroupingFromHealthCheckCollection();

    [Obsolete("Use CreateHealthCheckGroupWithResultViewModelAsync instead. Will be removed in v17.")]
    HealthCheckGroupWithResultResponseModel CreateHealthCheckGroupWithResultViewModel(IGrouping<string?, HealthCheck> healthCheckGroup);

    [Obsolete("Use CreateHealthCheckGroupWithResultViewModelAsync instead. Will be removed in v17.")]
    HealthCheckWithResultPresentationModel CreateHealthCheckWithResultViewModel(HealthCheck healthCheck);

    Task<HealthCheckGroupWithResultResponseModel> CreateHealthCheckGroupWithResultViewModelAsync(IGrouping<string?, HealthCheck> healthCheckGroup)
#pragma warning disable CS0618 // Type or member is obsolete
        => Task.FromResult(CreateHealthCheckGroupWithResultViewModel(healthCheckGroup));
#pragma warning restore CS0618 // Type or member is obsolete

    Task<HealthCheckWithResultPresentationModel> CreateHealthCheckWithResultViewModelAsync(HealthCheck healthCheck)
#pragma warning disable CS0618 // Type or member is obsolete
        => Task.FromResult(CreateHealthCheckWithResultViewModel(healthCheck));
#pragma warning restore CS0618 // Type or member is obsolete
}
