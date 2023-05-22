using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs;

/// <summary>
/// Runs a recurring background job inside a hosted service.
/// Generic version for DependencyInjection
/// </summary>
/// <typeparam name="TJob">Type of the Job</typeparam>
public class RecurringBackgroundJobHostedService<TJob> : RecurringBackgroundJobHostedService where TJob : IRecurringBackgroundJob
{
    public RecurringBackgroundJobHostedService(
        IRuntimeState runtimeState,
        ILogger<RecurringBackgroundJobHostedService<TJob>> logger,
        IMainDom mainDom,
        IServerRoleAccessor serverRoleAccessor,
        TJob job)
        : base(runtimeState, logger, mainDom, serverRoleAccessor, job)
    {

    }
}

/// <summary>
///    Runs a background job inside a hosted service.
/// </summary>
public class RecurringBackgroundJobHostedService : RecurringHostedServiceBase
{
    private readonly ILogger<RecurringBackgroundJobHostedService> _logger;
    private readonly IMainDom _mainDom;
    private readonly IRuntimeState _runtimeState;
    private readonly IServerRoleAccessor _serverRoleAccessor;
    private readonly IRecurringBackgroundJob _job;
    public string JobName { get => _job.GetType().Name; }


    /// <summary>
    ///     Initializes a new instance of the <see cref="RecurringBackgroundJobHostedService" /> class.
    /// </summary>
    public RecurringBackgroundJobHostedService(
        IRuntimeState runtimeState,
        ILogger<RecurringBackgroundJobHostedService> logger,
        IMainDom mainDom,
        IServerRoleAccessor serverRoleAccessor,
        IRecurringBackgroundJob job)
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
        if (_job.ServerRoles.Contains(_serverRoleAccessor.CurrentServerRole))
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
