// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Telemetry.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Telemetry;

/// <inheritdoc />
internal class TelemetryService : ITelemetryService
{
    private readonly IPackagingService _packagingService;
    private readonly IMetricsConsentService _metricsConsentService;
    private readonly ISiteIdentifierService _siteIdentifierService;
    private readonly IUmbracoVersion _umbracoVersion;
    private readonly IUsageInformationService _usageInformationService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TelemetryService" /> class.
    /// </summary>
    public TelemetryService(
        IPackagingService packagingService,
        IUmbracoVersion umbracoVersion,
        ISiteIdentifierService siteIdentifierService,
        IUsageInformationService usageInformationService,
        IMetricsConsentService metricsConsentService)
    {
        _packagingService = packagingService;
        _umbracoVersion = umbracoVersion;
        _siteIdentifierService = siteIdentifierService;
        _usageInformationService = usageInformationService;
        _metricsConsentService = metricsConsentService;
    }

    /// <inheritdoc />
    public bool TryGetTelemetryReportData(out TelemetryReportData? telemetryReportData)
    {
        if (_siteIdentifierService.TryGetOrCreateSiteIdentifier(out Guid telemetryId) is false)
        {
            telemetryReportData = null;
            return false;
        }

        telemetryReportData = new TelemetryReportData
        {
            Id = telemetryId,
            Version = GetVersion(),
            Packages = GetPackageTelemetry(),
            Detailed = _usageInformationService.GetDetailed(),
        };
        return true;
    }

    private string? GetVersion()
    {
        if (_metricsConsentService.GetConsentLevel() == TelemetryLevel.Minimal)
        {
            return null;
        }

        return _umbracoVersion.SemanticVersion.ToSemanticStringWithoutBuild();
    }

    private IEnumerable<PackageTelemetry>? GetPackageTelemetry()
    {
        if (_metricsConsentService.GetConsentLevel() == TelemetryLevel.Minimal)
        {
            return null;
        }

        List<PackageTelemetry> packages = new();
        IEnumerable<InstalledPackage> installedPackages = _packagingService.GetAllInstalledPackages();

        foreach (InstalledPackage installedPackage in installedPackages)
        {
            if (installedPackage.AllowPackageTelemetry is false)
            {
                continue;
            }

            packages.Add(new PackageTelemetry
            {
                Id = installedPackage.PackageId,
                Name = installedPackage.PackageName,
                Version = installedPackage.Version,
            });
        }

        return packages;
    }
}
