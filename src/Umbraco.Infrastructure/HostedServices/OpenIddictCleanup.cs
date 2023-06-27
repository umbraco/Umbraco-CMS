using System.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.HostedServices;

// port of the OpenIddict Quartz job for cleaning up - see https://github.com/openiddict/openiddict-core/tree/dev/src/OpenIddict.Quartz
public class OpenIddictCleanup : RecurringHostedServiceBase
{
    // keep tokens and authorizations in the database for 7 days
    // - NOTE: this is NOT the same as access token lifetime, which is likely very short
    private const int LifespanInSeconds = 7 * 24 * 60 * 60;

    private readonly ILogger<OpenIddictCleanup> _logger;
    private readonly IServiceProvider _provider;
    private readonly IRuntimeState _runtimeState;

    public OpenIddictCleanup(
        ILogger<OpenIddictCleanup> logger, IServiceProvider provider, IRuntimeState runtimeState)
        : base(logger, TimeSpan.FromHours(1), TimeSpan.FromMinutes(5))
    {
        _logger = logger;
        _provider = provider;
        _runtimeState = runtimeState;
    }

    public override async Task PerformExecuteAsync(object? state)
    {
        if (_runtimeState.Level < RuntimeLevel.Run)
        {
            return;
        }

        // hosted services are registered as singletons, but this particular one consumes scoped services... so
        // we have to fetch the service dependencies manually using a new scope per invocation.
        IServiceScope scope = _provider.CreateScope();
        DateTimeOffset threshold = DateTimeOffset.UtcNow - TimeSpan.FromSeconds(LifespanInSeconds);

        try
        {
            IOpenIddictTokenManager tokenManager = scope.ServiceProvider.GetService<IOpenIddictTokenManager>()
                    ?? throw new ConfigurationErrorsException($"Could not retrieve an {nameof(IOpenIddictTokenManager)} service from the current scope");
            await tokenManager.PruneAsync(threshold);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to prune OpenIddict tokens");
        }

        try
        {
            IOpenIddictAuthorizationManager authorizationManager = scope.ServiceProvider.GetService<IOpenIddictAuthorizationManager>()
                    ?? throw new ConfigurationErrorsException($"Could not retrieve an {nameof(IOpenIddictAuthorizationManager)} service from the current scope");
            await authorizationManager.PruneAsync(threshold);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to prune OpenIddict authorizations");
        }
    }
}
