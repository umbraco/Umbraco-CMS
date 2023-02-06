using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.HealthCheck;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Factories;

public class HealthCheckGroupWithResultViewModelFactory : IHealthCheckGroupWithResultViewModelFactory
{
    private readonly HealthChecksSettings _healthChecksSettings;
    private readonly ILogger<IHealthCheckGroupWithResultViewModelFactory> _logger;
    private readonly IUmbracoMapper _umbracoMapper;

    public HealthCheckGroupWithResultViewModelFactory(
        IOptions<HealthChecksSettings> healthChecksSettings,
        ILogger<IHealthCheckGroupWithResultViewModelFactory> logger,
        IUmbracoMapper umbracoMapper)
    {
        _healthChecksSettings = healthChecksSettings.Value;
        _logger = logger;
        _umbracoMapper = umbracoMapper;
    }

    public IEnumerable<IGrouping<string?, HealthCheck>> CreateGroupingFromHealthCheckCollection(HealthCheckCollection healthChecks)
    {
        IList<Guid> disabledCheckIds = _healthChecksSettings.DisabledChecks
            .Select(x => x.Id)
            .ToList();

        IEnumerable<IGrouping<string?, HealthCheck>> groups = healthChecks
            .Where(x => disabledCheckIds.Contains(x.Id) == false)
            .GroupBy(x => x.Group)
            .OrderBy(x => x.Key);

        return groups;
    }

    public HealthCheckGroupWithResultViewModel CreateHealthCheckGroupWithResultViewModel(IGrouping<string?, HealthCheck> healthCheckGroup)
    {
        var healthChecks = new List<HealthCheckWithResultViewModel>();

        foreach (HealthCheck healthCheck in healthCheckGroup)
        {
            healthChecks.Add(CreateHealthCheckWithResultViewModel(healthCheck));
        }

        var healthCheckGroupViewModel = new HealthCheckGroupWithResultViewModel
        {
            Name = healthCheckGroup.Key,
            Checks = healthChecks
        };

        return healthCheckGroupViewModel;
    }

    public HealthCheckWithResultViewModel CreateHealthCheckWithResultViewModel(HealthCheck healthCheck)
    {
        _logger.LogDebug("Running health check: " + healthCheck.Name);

        IEnumerable<HealthCheckStatus> results = healthCheck.GetStatus().Result;

        var healthCheckViewModel = new HealthCheckWithResultViewModel
        {
            Key = healthCheck.Id,
            Name = healthCheck.Name,
            Description = healthCheck.Description,
            Results = _umbracoMapper.MapEnumerable<HealthCheckStatus, HealthCheckResultViewModel>(results)
        };

        return healthCheckViewModel;
    }
}
