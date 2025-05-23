using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Core.Services;

public class MetricsConsentService : IMetricsConsentService
{
    internal const string Key = "UmbracoAnalyticsLevel";

    private readonly IKeyValueService _keyValueService;
    private readonly ILogger<MetricsConsentService> _logger;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserService _userService;

    public MetricsConsentService(
        IKeyValueService keyValueService,
        ILogger<MetricsConsentService> logger,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserService userService)
    {
        _keyValueService = keyValueService;
        _logger = logger;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userService = userService;
    }

    public TelemetryLevel GetConsentLevel()
    {
        var analyticsLevelString = _keyValueService.GetValue(Key);

        if (analyticsLevelString is null ||
            Enum.TryParse(analyticsLevelString, out TelemetryLevel analyticsLevel) is false)
        {
            return TelemetryLevel.Detailed;
        }

        return analyticsLevel;
    }

    public async Task SetConsentLevelAsync(TelemetryLevel telemetryLevel)
    {
        IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser ?? await _userService.GetAsync(Constants.Security.SuperUserKey);

        _logger.LogInformation("Telemetry level set to {telemetryLevel} by {username}", telemetryLevel, currentUser?.Username);
        _keyValueService.SetValue(Key, telemetryLevel.ToString());
    }
}
