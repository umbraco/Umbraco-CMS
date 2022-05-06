using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.Services;

public class MetricsConsentService : IMetricsConsentService
{
    internal const string Key = "UmbracoAnalyticsLevel";
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    private readonly IKeyValueService _keyValueService;
    private readonly ILogger<MetricsConsentService> _logger;

    // Scheduled for removal in V12
    [Obsolete("Please use the constructor that takes and ILogger and IBackOfficeSecurity instead")]
    public MetricsConsentService(IKeyValueService keyValueService)
        : this(
            keyValueService,
            StaticServiceProvider.Instance.GetRequiredService<ILogger<MetricsConsentService>>(),
            StaticServiceProvider.Instance.GetRequiredService<IBackOfficeSecurityAccessor>())
    {
    }

    public MetricsConsentService(
        IKeyValueService keyValueService,
        ILogger<MetricsConsentService> logger,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _keyValueService = keyValueService;
        _logger = logger;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    public TelemetryLevel GetConsentLevel()
    {
        var analyticsLevelString = _keyValueService.GetValue(Key);

        if (analyticsLevelString is null ||
            Enum.TryParse(analyticsLevelString, out TelemetryLevel analyticsLevel) is false)
        {
            return TelemetryLevel.Basic;
        }

        return analyticsLevel;
    }

    public void SetConsentLevel(TelemetryLevel telemetryLevel)
    {
        IUser currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        _logger.LogInformation("Telemetry level set to {telemetryLevel} by {username}", telemetryLevel,
            currentUser?.Username);
        _keyValueService.SetValue(Key, telemetryLevel.ToString());
    }
}
