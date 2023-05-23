using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs;

public static class RecurringBackgroundJobHostedService
{
    public static Func<IRecurringBackgroundJob, IHostedService> CreateHostedServiceFactory(IServiceProvider serviceProvider) =>
        (IRecurringBackgroundJob job) =>
        {
            Type hostedServiceType = typeof(RecurringBackgroundJobHostedService<>).MakeGenericType(job.GetType());
            return (IHostedService)ActivatorUtilities.CreateInstance(serviceProvider, hostedServiceType, job);
        };
}

/// <summary>
/// Runs a recurring background job inside a hosted service.
/// Generic version for DependencyInjection
/// </summary>
/// <typeparam name="TJob">Type of the Job</typeparam>
public class RecurringBackgroundJobHostedService<TJob> : RecurringHostedServiceBase where TJob : IRecurringBackgroundJob
{

    private readonly ILogger<RecurringBackgroundJobHostedService<TJob>> _logger;
    private readonly IMainDom _mainDom;
    private readonly IRuntimeState _runtimeState;
    private readonly IServerRoleAccessor _serverRoleAccessor;
    private readonly IRecurringBackgroundJob _job;

    public RecurringBackgroundJobHostedService(
        IRuntimeState runtimeState,
        ILogger<RecurringBackgroundJobHostedService<TJob>> logger,
        IMainDom mainDom,
        IServerRoleAccessor serverRoleAccessor,
        TJob job)
        : base(logger, job.Period, job.Delay)
    {
        _runtimeState = runtimeState;
        _logger = logger;
        _mainDom = mainDom;
        _serverRoleAccessor = serverRoleAccessor;
        _job = job;

        _job.PeriodChanged += (sender, e) => ChangePeriod(_job.Period);
    }

    /// <inheritdoc />
    public override async Task PerformExecuteAsync(object? state)
    {

        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            _logger.LogDebug("Job not running as runlevel not yet ready");
            return;
        }

        // Don't run on replicas nor unknown role servers
        if (!_job.ServerRoles.Contains(_serverRoleAccessor.CurrentServerRole))
        {
            _logger.LogDebug("Job not running on this server role");
            return;
        }

        // Ensure we do not run if not main domain, but do NOT lock it
        if (!_mainDom.IsMainDom)
        {
            _logger.LogDebug("Job not running as not MainDom");
            return;
        }

        await _job.RunJobAsync();
    }
}
