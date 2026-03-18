// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs.ServerRegistration;

/// <summary>
///     Implements periodic server "touching" (to mark as active/deactive) as a hosted service.
/// </summary>
public class TouchServerJob : IRecurringBackgroundJob
{
    /// <summary>
    /// Gets the period that defines how often the server should be touched.
    /// </summary>
    public TimeSpan Period { get; private set; }

    /// <summary>
    /// Gets the fixed delay interval of 15 seconds between executions of the touch server job.
    /// This interval determines how often the server registration is updated.
    /// </summary>
    public TimeSpan Delay { get => TimeSpan.FromSeconds(15); }

    /// <summary>
    /// Gets all server roles on which this job runs. This property returns every possible <see cref="ServerRole"/> value, indicating the job runs on all server roles.
    /// </summary>
    /// <remarks>Runs on all servers</remarks>
    public ServerRole[] ServerRoles { get => Enum.GetValues<ServerRole>(); }

    private event EventHandler? _periodChanged;

    /// <summary>
    /// Occurs when the period of the TouchServerJob changes.
    /// </summary>
    public event EventHandler PeriodChanged
    {
        add { _periodChanged += value; }
        remove { _periodChanged -= value; }
    }


    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILogger<TouchServerJob> _logger;
    private readonly IServerRegistrationService _serverRegistrationService;
    private readonly IServerRoleAccessor _serverRoleAccessor;
    private GlobalSettings _globalSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TouchServerJob" /> class.
    /// </summary>
    /// <param name="serverRegistrationService">Services for server registrations.</param>
    /// <param name="logger">The typed logger.</param>
    /// <param name="globalSettings">The configuration for global settings.</param>
    /// <param name="hostingEnvironment">The hostingEnviroment.</param>
    /// <param name="serverRoleAccessor">The accessor for the server role</param>
    public TouchServerJob(
        IServerRegistrationService serverRegistrationService,
        IHostingEnvironment hostingEnvironment,
        ILogger<TouchServerJob> logger,
        IOptionsMonitor<GlobalSettings> globalSettings,
        IServerRoleAccessor serverRoleAccessor)
    {
        _serverRegistrationService = serverRegistrationService ??
                                     throw new ArgumentNullException(nameof(serverRegistrationService));
        _hostingEnvironment = hostingEnvironment;
        _logger = logger;
        _globalSettings = globalSettings.CurrentValue;
        _serverRoleAccessor = serverRoleAccessor;

        Period = _globalSettings.DatabaseServerRegistrar.WaitTimeBetweenCalls;
        globalSettings.OnChange(x =>
        {
            _globalSettings = x;
            Period = x.DatabaseServerRegistrar.WaitTimeBetweenCalls;

            _periodChanged?.Invoke(this, EventArgs.Empty);
        });
    }

    /// <summary>
    /// Executes the job that updates the server registration by touching the server record in the database.
    /// This keeps the server's registration active and ensures its status remains current.
    /// </summary>
    /// <returns>A completed task when the job has finished running.</returns>
    public Task RunJobAsync()
    {

        // If the IServerRoleAccessor has been changed away from ElectedServerRoleAccessor this task no longer makes sense,
        // since all it's used for is to allow the ElectedServerRoleAccessor
        // to figure out what role a given server has, so we just stop this task.
        if (_serverRoleAccessor is not ElectedServerRoleAccessor)
        {
            return Task.CompletedTask;
        }

        var serverAddress = _hostingEnvironment.ApplicationMainUrl?.ToString();
        if (serverAddress.IsNullOrWhiteSpace())
        {
            _logger.LogWarning("No umbracoApplicationUrl for service (yet), skip.");
            return Task.CompletedTask;
        }

        try
        {
            _serverRegistrationService.TouchServer(
                serverAddress!,
                _globalSettings.DatabaseServerRegistrar.StaleServerTimeout);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update server record in database.");
        }

        return Task.CompletedTask;
    }
}
