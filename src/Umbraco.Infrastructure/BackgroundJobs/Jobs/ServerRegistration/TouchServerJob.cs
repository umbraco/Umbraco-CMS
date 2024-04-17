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
    public TimeSpan Period { get; private set; }
    public TimeSpan Delay { get => TimeSpan.FromSeconds(15); }

    // Runs on all servers
    public ServerRole[] ServerRoles { get => Enum.GetValues<ServerRole>(); }

    private event EventHandler? _periodChanged;
    public event EventHandler PeriodChanged {
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
