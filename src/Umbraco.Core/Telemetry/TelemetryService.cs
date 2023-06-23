// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Telemetry.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Telemetry;

/// <inheritdoc />
internal class TelemetryService : ITelemetryService
{
    private readonly IMetricsConsentService _metricsConsentService;
    private readonly ISiteIdentifierService _siteIdentifierService;
    private readonly IUmbracoVersion _umbracoVersion;
    private readonly IUsageInformationService _usageInformationService;
    private readonly IPackageManifestService _packageManifestService;

    [Obsolete("Please use the constructor that does not take an IManifestParser. Will be removed in V15.")]
    public TelemetryService(
        ILegacyManifestParser legacyManifestParser,
        IUmbracoVersion umbracoVersion,
        ISiteIdentifierService siteIdentifierService,
        IUsageInformationService usageInformationService,
        IMetricsConsentService metricsConsentService)
        : this(
            legacyManifestParser,
            umbracoVersion,
            siteIdentifierService,
            usageInformationService,
            metricsConsentService,
            StaticServiceProvider.Instance.GetRequiredService<IPackageManifestService>())
    {
    }

    [Obsolete("Please use the constructor that does not take an IManifestParser. Will be removed in V15.")]
    public TelemetryService(
        ILegacyManifestParser legacyManifestParser,
        IUmbracoVersion umbracoVersion,
        ISiteIdentifierService siteIdentifierService,
        IUsageInformationService usageInformationService,
        IMetricsConsentService metricsConsentService,
        IPackageManifestService packageManifestService)
        : this(
            umbracoVersion,
            siteIdentifierService,
            usageInformationService,
            metricsConsentService,
            packageManifestService)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TelemetryService" /> class.
    /// </summary>
    public TelemetryService(
        IUmbracoVersion umbracoVersion,
        ISiteIdentifierService siteIdentifierService,
        IUsageInformationService usageInformationService,
        IMetricsConsentService metricsConsentService,
        IPackageManifestService packageManifestService)
    {
        _umbracoVersion = umbracoVersion;
        _siteIdentifierService = siteIdentifierService;
        _usageInformationService = usageInformationService;
        _metricsConsentService = metricsConsentService;
        _packageManifestService = packageManifestService;
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
            Packages = await GetPackageTelemetryAsync(),
            Detailed = _usageInformationService.GetDetailed(),
        };
    }

    private string? GetVersion() => _metricsConsentService.GetConsentLevel() == TelemetryLevel.Minimal
        ? null
        : _umbracoVersion.SemanticVersion.ToSemanticStringWithoutBuild();

    private async Task<IEnumerable<PackageTelemetry>?> GetPackageTelemetryAsync()
    {
        if (_metricsConsentService.GetConsentLevel() == TelemetryLevel.Minimal)
        {
            return null;
        }

        IEnumerable<PackageManifest> manifests = await _packageManifestService.GetPackageManifestsAsync();

        return manifests
            .Where(manifest => manifest.AllowTelemetry)
            .Select(manifest => new PackageTelemetry
            {
                Name = manifest.Name,
                Version = manifest.Version ?? string.Empty
            });
    }
}
