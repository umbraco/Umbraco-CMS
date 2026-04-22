using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.HealthCheck;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Factories;

// TODO (V16): Make internal sealed.
/// <summary>
/// Factory for creating health check group presentation models.
/// </summary>
public class HealthCheckGroupPresentationFactory : IHealthCheckGroupPresentationFactory
{
    private readonly HealthChecksSettings _healthChecksSettings;
    private readonly HealthCheckCollection _healthChecks;
    private readonly ILogger<IHealthCheckGroupPresentationFactory> _logger;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthCheckGroupPresentationFactory"/> class.
    /// </summary>
    /// <param name="healthChecksSettings">The <see cref="IOptions{HealthChecksSettings}"/> containing configuration for health checks.</param>
    /// <param name="healthChecks">The <see cref="HealthCheckCollection"/> representing the available health checks.</param>
    /// <param name="logger">The <see cref="ILogger{IHealthCheckGroupPresentationFactory}"/> used for logging.</param>
    /// <param name="umbracoMapper">The <see cref="IUmbracoMapper"/> instance used for mapping objects.</param>
    public HealthCheckGroupPresentationFactory(
        IOptions<HealthChecksSettings> healthChecksSettings,
        HealthCheckCollection healthChecks,
        ILogger<IHealthCheckGroupPresentationFactory> logger,
        IUmbracoMapper umbracoMapper)
    {
        _healthChecksSettings = healthChecksSettings.Value;
        _healthChecks = healthChecks;
        _logger = logger;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Groups enabled health checks by their group name, excluding any checks that are disabled in the settings.
    /// </summary>
    /// <returns>An ordered enumerable of groupings, where each grouping's key is the group name and the elements are the health checks in that group.</returns>
    public IEnumerable<IGrouping<string?, HealthCheck>> CreateGroupingFromHealthCheckCollection()
    {
        IList<Guid> disabledCheckIds = _healthChecksSettings.DisabledChecks
            .Select(x => x.Id)
            .ToList();

        IEnumerable<IGrouping<string?, HealthCheck>> groups = _healthChecks
            .Where(x => disabledCheckIds.Contains(x.Id) == false)
            .GroupBy(x => x.Group)
            .OrderBy(x => x.Key);

        return groups;
    }

    /// <summary>
    /// Asynchronously creates a response model representing a group of health checks and their results.
    /// </summary>
    /// <param name="healthCheckGroup">An <see cref="IGrouping{TKey, TElement}"/> containing the health checks to include in the group, grouped by their key.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="HealthCheckGroupWithResultResponseModel"/> with the health check group and its results.</returns>
    public async Task<HealthCheckGroupWithResultResponseModel> CreateHealthCheckGroupWithResultViewModelAsync(IGrouping<string?, HealthCheck> healthCheckGroup)
    {
        var healthChecks = new List<HealthCheckWithResultPresentationModel>();

        foreach (HealthCheck healthCheck in healthCheckGroup)
        {
            healthChecks.Add(await CreateHealthCheckWithResultViewModelAsync(healthCheck));
        }

        var healthCheckGroupViewModel = new HealthCheckGroupWithResultResponseModel
        {
            Checks = healthChecks
        };

        return healthCheckGroupViewModel;
    }

    /// <summary>
    /// Asynchronously creates a <see cref="HealthCheckWithResultPresentationModel"/> view model for the specified <see cref="HealthCheck"/>, including the results obtained by executing the health check.
    /// </summary>
    /// <param name="healthCheck">The <see cref="HealthCheck"/> instance to execute and generate the view model for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a <see cref="HealthCheckWithResultPresentationModel"/> populated with the health check's results.
    /// </returns>
    public async Task<HealthCheckWithResultPresentationModel> CreateHealthCheckWithResultViewModelAsync(HealthCheck healthCheck)
    {
        _logger.LogDebug($"Running health check: {healthCheck.Name}");

        IEnumerable<HealthCheckStatus> results = await healthCheck.GetStatusAsync();

        var healthCheckViewModel = new HealthCheckWithResultPresentationModel
        {
            Id = healthCheck.Id,
            Results = _umbracoMapper.MapEnumerable<HealthCheckStatus, HealthCheckResultResponseModel>(results)
        };

        return healthCheckViewModel;
    }
}
