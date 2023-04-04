using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.HealthCheck;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Factories;

public class HealthCheckGroupPresentationFactory : IHealthCheckGroupPresentationFactory
{
    private readonly HealthChecksSettings _healthChecksSettings;
    private readonly HealthCheckCollection _healthChecks;
    private readonly ILogger<IHealthCheckGroupPresentationFactory> _logger;
    private readonly IUmbracoMapper _umbracoMapper;

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

    public HealthCheckGroupWithResultResponseModel CreateHealthCheckGroupWithResultViewModel(IGrouping<string?, HealthCheck> healthCheckGroup)
    {
        var healthChecks = new List<HealthCheckWithResultPresentationModel>();

        foreach (HealthCheck healthCheck in healthCheckGroup)
        {
            healthChecks.Add(CreateHealthCheckWithResultViewModel(healthCheck));
        }

        var healthCheckGroupViewModel = new HealthCheckGroupWithResultResponseModel
        {
            Checks = healthChecks
        };

        return healthCheckGroupViewModel;
    }

    public HealthCheckWithResultPresentationModel CreateHealthCheckWithResultViewModel(HealthCheck healthCheck)
    {
        _logger.LogDebug($"Running health check: {healthCheck.Name}");

        IEnumerable<HealthCheckStatus> results = healthCheck.GetStatus().Result;

        var healthCheckViewModel = new HealthCheckWithResultPresentationModel
        {
            Id = healthCheck.Id,
            Results = _umbracoMapper.MapEnumerable<HealthCheckStatus, HealthCheckResultResponseModel>(results)
        };

        return healthCheckViewModel;
    }
}
