using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Service for managing telemetry consent levels for Umbraco analytics.
/// </summary>
public class MetricsConsentService : IMetricsConsentService
{
    /// <summary>
    ///     The key used to store the analytics level in the key-value store.
    /// </summary>
    internal const string Key = "UmbracoAnalyticsLevel";

    private readonly IKeyValueService _keyValueService;
    private readonly ILogger<MetricsConsentService> _logger;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserService _userService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MetricsConsentService" /> class.
    /// </summary>
    /// <param name="keyValueService">The key-value service for storing consent settings.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="backOfficeSecurityAccessor">The back office security accessor.</param>
    /// <param name="userService">The user service.</param>
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public Task SetConsentLevelAsync(TelemetryLevel telemetryLevel)
    {
        _logger.LogInformation("Telemetry level set to {telemetryLevel}", telemetryLevel);
        _keyValueService.SetValue(Key, telemetryLevel.ToString());
        return Task.CompletedTask;
    }
}
