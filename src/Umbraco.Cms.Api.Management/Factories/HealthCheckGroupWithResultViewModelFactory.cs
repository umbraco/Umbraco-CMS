using Microsoft.Extensions.Logging;
using Umbraco.Cms.Api.Management.ViewModels.HealthCheck;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Factories;

public class HealthCheckGroupWithResultViewModelFactory : IHealthCheckGroupWithResultViewModelFactory
{
    private readonly ILogger<IHealthCheckGroupWithResultViewModelFactory> _logger;
    private readonly IUmbracoMapper _umbracoMapper;

    public HealthCheckGroupWithResultViewModelFactory(ILogger<IHealthCheckGroupWithResultViewModelFactory> logger, IUmbracoMapper umbracoMapper)
    {
        _logger = logger;
        _umbracoMapper = umbracoMapper;
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
