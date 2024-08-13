using System.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;

// port of the OpenIddict Quartz job for cleaning up - see https://github.com/openiddict/openiddict-core/tree/dev/src/OpenIddict.Quartz
public class OpenIddictCleanupJob : IRecurringBackgroundJob
{
    public TimeSpan Period { get => TimeSpan.FromHours(1); }
    public TimeSpan Delay { get => TimeSpan.FromMinutes(5); }

    // No-op event as the period never changes on this job
    public event EventHandler PeriodChanged { add { } remove { } }


    // keep tokens and authorizations in the database for 7 days
    // - NOTE: this is NOT the same as access token lifetime, which is likely very short
    private const int LifespanInSeconds = 7 * 24 * 60 * 60;

    private readonly ILogger<OpenIddictCleanupJob> _logger;
    private readonly IServiceProvider _provider;

    public OpenIddictCleanupJob(ILogger<OpenIddictCleanupJob> logger, IServiceProvider provider)
    {
        _logger = logger;
        _provider = provider;
    }

    public async Task RunJobAsync()
    {
        // hosted services are registered as singletons, but this particular one consumes scoped services... so
        // we have to fetch the service dependencies manually using a new scope per invocation.
        IServiceScope scope = _provider.CreateScope();
        DateTimeOffset threshold = DateTimeOffset.UtcNow - TimeSpan.FromSeconds(LifespanInSeconds);

        try
        {
            IOpenIddictTokenManager tokenManager = scope.ServiceProvider.GetService<IOpenIddictTokenManager>()
                                                   ?? throw new InvalidOperationException($"Could not retrieve an {nameof(IOpenIddictTokenManager)} service from the current scope");
            await tokenManager.PruneAsync(threshold);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to prune OpenIddict tokens");
        }

        try
        {
            IOpenIddictAuthorizationManager authorizationManager = scope.ServiceProvider.GetService<IOpenIddictAuthorizationManager>()
                                                                   ?? throw new InvalidOperationException($"Could not retrieve an {nameof(IOpenIddictAuthorizationManager)} service from the current scope");
            await authorizationManager.PruneAsync(threshold);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to prune OpenIddict authorizations");
        }
    }
}
