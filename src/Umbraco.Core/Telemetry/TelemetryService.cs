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

    [Obsolete("Please use GetTelemetryReportDataAsync. Will be removed in V15.")]
    public bool TryGetTelemetryReportData(out TelemetryReportData? telemetryReportData)
    {
        telemetryReportData = GetTelemetryReportDataAsync().GetAwaiter().GetResult();

        return telemetryReportData != null;
    }

    /// <inheritdoc />
    public async Task<TelemetryReportData?> GetTelemetryReportDataAsync()
    {
        if (_siteIdentifierService.TryGetOrCreateSiteIdentifier(out Guid telemetryId) is false)
        {
            return null;
        }

        return new TelemetryReportData
        {
            Id = telemetryId,
            Version = GetVersion(),
            Packages = await GetPackageTelemetryAsync().ConfigureAwait(false),
            Detailed = _usageInformationService.GetDetailed(),
        };
    }

    private string? GetVersion()
        => _metricsConsentService.GetConsentLevel() == TelemetryLevel.Minimal
        ? null
        : _umbracoVersion.SemanticVersion.ToSemanticStringWithoutBuild();

    private async Task<IEnumerable<PackageTelemetry>?> GetPackageTelemetryAsync()
    {
        if (_metricsConsentService.GetConsentLevel() == TelemetryLevel.Minimal)
        {
            return null;
        }

        List<PackageTelemetry> packages = new();
        IEnumerable<InstalledPackage> installedPackages = await _packagingService.GetAllInstalledPackagesAsync().ConfigureAwait(false);

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
