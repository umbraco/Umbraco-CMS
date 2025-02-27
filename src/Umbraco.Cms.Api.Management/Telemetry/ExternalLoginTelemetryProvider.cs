using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Api.Management.Telemetry;

public class ExternalLoginTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly
        IBackOfficeExternalLoginProviders _externalLoginProviders;

    public ExternalLoginTelemetryProvider(IBackOfficeExternalLoginProviders externalLoginProviders)
    {
        _externalLoginProviders = externalLoginProviders;
    }

    public IEnumerable<UsageInformation> GetInformation()
    {
        IEnumerable<BackOfficeExternaLoginProviderScheme> providers = _externalLoginProviders.GetBackOfficeProvidersAsync().GetAwaiter().GetResult();
        yield return new UsageInformation(Constants.Telemetry.BackofficeExternalLoginProviderCount, providers.Count());
    }
}
