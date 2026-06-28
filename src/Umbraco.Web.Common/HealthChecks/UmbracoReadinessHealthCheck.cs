using Microsoft.Extensions.Diagnostics.HealthChecks;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.Common.HealthChecks;

/// <summary>
/// ASP.NET Core health check that reports readiness based on the Umbraco runtime level.
/// Reports <see cref="HealthCheckResult.Healthy"/> when the runtime level is <see cref="RuntimeLevel.Run"/>,
/// otherwise <see cref="HealthCheckResult.Unhealthy"/> (mapped to HTTP 503 by the default status-code mapping)
/// so the endpoint signals "not ready" during startup and unattended upgrades.
/// </summary>
internal sealed class UmbracoReadinessHealthCheck : IHealthCheck
{
    internal const string ReadyTag = "umbraco-ready";

    private readonly IRuntimeState _runtimeState;

    public UmbracoReadinessHealthCheck(IRuntimeState runtimeState)
        => _runtimeState = runtimeState;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        => Task.FromResult(
            _runtimeState.Level == RuntimeLevel.Run
                ? HealthCheckResult.Healthy("Umbraco is ready.")
                : HealthCheckResult.Unhealthy($"Umbraco is not yet ready. Level: {_runtimeState.Level}"));
}
