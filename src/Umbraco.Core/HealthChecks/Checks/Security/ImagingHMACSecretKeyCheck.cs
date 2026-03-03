// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.HealthChecks.Checks.Security;

/// <summary>
///     Health check for the HMAC secret key used to authenticate image manipulation URLs.
/// </summary>
[HealthCheck(
    "A3E3A6C8-3D42-4F6E-B285-9F6A5C7E1A14",
    "Imaging HMAC Secret Key",
    Description = "Verifies that an HMAC secret key is configured to authenticate and protect image manipulation URLs.",
    Group = "Security")]
public class ImagingHMACSecretKeyCheck : HealthCheck
{
    private readonly ILocalizedTextService _textService;
    private readonly IOptionsMonitor<ImagingSettings> _imagingSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ImagingHMACSecretKeyCheck" /> class.
    /// </summary>
    /// <param name="textService">The localized text service.</param>
    /// <param name="imagingSettings">The imaging settings monitor.</param>
    public ImagingHMACSecretKeyCheck(
        ILocalizedTextService textService,
        IOptionsMonitor<ImagingSettings> imagingSettings)
    {
        _textService = textService;
        _imagingSettings = imagingSettings;
    }

    /// <inheritdoc />
    public override Task<IEnumerable<HealthCheckStatus>> GetStatusAsync()
    {
        bool isConfigured = _imagingSettings.CurrentValue.HMACSecretKey?.Length > 0;

        HealthCheckStatus status = isConfigured
            ? new HealthCheckStatus(_textService.Localize("healthcheck", "imagingHMACSecretKeyCheckSuccessMessage"))
              {
                  ResultType = StatusResultType.Success,
              }
            : new HealthCheckStatus(_textService.Localize("healthcheck", "imagingHMACSecretKeyCheckWarningMessage"))
              {
                  ResultType = StatusResultType.Warning,
                  ReadMoreLink = Constants.HealthChecks.DocumentationLinks.Security.ImagingHMACSecretKeyCheck,
              };

        return Task.FromResult(status.Yield());
    }

    /// <inheritdoc />
    public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        => throw new NotSupportedException("Configuration cannot be automatically fixed.");
}
