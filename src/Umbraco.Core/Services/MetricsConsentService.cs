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

    private readonly IKeyValueService _keyValueService;
    private readonly ILogger<MetricsConsentService> _logger;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserService _userService;

    // Scheduled for removal in V12
    [Obsolete("Please use the constructor that takes an ILogger and IBackOfficeSecurity instead")]
    public MetricsConsentService(IKeyValueService keyValueService)
        : this(
            keyValueService,
            StaticServiceProvider.Instance.GetRequiredService<ILogger<MetricsConsentService>>(),
            StaticServiceProvider.Instance.GetRequiredService<IBackOfficeSecurityAccessor>(),
            StaticServiceProvider.Instance.GetRequiredService<IUserService>())
    {
    }

    // Scheduled for removal in V12
    [Obsolete("Please use the constructor that takes an IUserService instead")]
    public MetricsConsentService(
        IKeyValueService keyValueService,
        ILogger<MetricsConsentService> logger,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : this(
            keyValueService,
            logger,
            backOfficeSecurityAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IUserService>())
    {
    }

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
            return TelemetryLevel.Basic;
        }

        return analyticsLevel;
    }

    public void SetConsentLevel(TelemetryLevel telemetryLevel)
    {
        IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        if (currentUser is null)
        {
            currentUser = _userService.GetUserById(Constants.Security.SuperUserId);
        }

        _logger.LogInformation("Telemetry level set to {telemetryLevel} by {username}", telemetryLevel, currentUser?.Username);
        _keyValueService.SetValue(Key, telemetryLevel.ToString());
    }
}
