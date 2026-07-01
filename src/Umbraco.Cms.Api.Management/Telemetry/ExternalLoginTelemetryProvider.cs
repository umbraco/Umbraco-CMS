using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Api.Management.Telemetry;

/// <summary>
/// Provides functionality for collecting and reporting telemetry data related to external login events within the system.
/// </summary>
public class ExternalLoginTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly
        IBackOfficeExternalLoginProviders _externalLoginProviders;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalLoginTelemetryProvider"/> class.
    /// </summary>
    /// <param name="externalLoginProviders">
    /// An instance of <see cref="IBackOfficeExternalLoginProviders"/> representing the available external login providers for the back office.
    /// </param>
    public ExternalLoginTelemetryProvider(IBackOfficeExternalLoginProviders externalLoginProviders)
    {
        _externalLoginProviders = externalLoginProviders;
    }

    /// <summary>
    /// Retrieves telemetry usage information related to external login providers configured for the back office.
    /// </summary>
    /// <returns>
    /// An enumerable collection containing usage information instances, such as the count of configured back office external login providers.
    /// </returns>
    public IEnumerable<UsageInformation> GetInformation()
    {
        IEnumerable<BackOfficeExternaLoginProviderScheme> providers = _externalLoginProviders.GetBackOfficeProvidersAsync().GetAwaiter().GetResult();
        yield return new UsageInformation(Constants.Telemetry.BackofficeExternalLoginProviderCount, providers.Count());
    }
}
