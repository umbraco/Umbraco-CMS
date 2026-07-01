using Umbraco.Cms.Api.Management.ViewModels.HealthCheck;
using Umbraco.Cms.Core.HealthChecks;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Provides an abstraction for creating presentation models of health check groups.
/// </summary>
public interface IHealthCheckGroupPresentationFactory
{
    /// <summary>
    /// Groups health checks from the collection by a specified key, such as category or group name.
    /// </summary>
    /// <returns>An enumerable of groupings, where each grouping's key represents a group identifier (e.g., category name), and the elements are <see cref="HealthCheck"/> items belonging to that group.</returns>
    IEnumerable<IGrouping<string?, HealthCheck>> CreateGroupingFromHealthCheckCollection();

    /// <summary>
    /// Asynchronously creates a view model representing a health check group and its results.
    /// </summary>
    /// <param name="healthCheckGroup">The group of health checks to generate the view model for.</param>
    /// <returns>A task that, when completed, contains the response model for the health check group with its results.</returns>
    Task<HealthCheckGroupWithResultResponseModel> CreateHealthCheckGroupWithResultViewModelAsync(
        IGrouping<string?, HealthCheck> healthCheckGroup);

    /// <summary>
    /// Asynchronously creates a <see cref="HealthCheckWithResultPresentationModel" /> from the specified <see cref="HealthCheck" />.
    /// </summary>
    /// <param name="healthCheck">The <see cref="HealthCheck" /> to create the presentation model for.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the created <see cref="HealthCheckWithResultPresentationModel" />.</returns>
    Task<HealthCheckWithResultPresentationModel> CreateHealthCheckWithResultViewModelAsync(HealthCheck healthCheck);
}
