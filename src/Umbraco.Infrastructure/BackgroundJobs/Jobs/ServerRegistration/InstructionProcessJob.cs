// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs.ServerRegistration;

/// <summary>
///     Implements periodic database instruction processing as a hosted service.
/// </summary>
public class InstructionProcessJob : IRecurringBackgroundJob
{

    public TimeSpan Period { get; }
    public TimeSpan Delay { get => TimeSpan.FromMinutes(1); }
    public ServerRole[] ServerRoles { get => Enum.GetValues<ServerRole>(); }

    // No-op event as the period never changes on this job
    public event EventHandler PeriodChanged { add { } remove { } }

    private readonly ILogger<InstructionProcessJob> _logger;
    private readonly IServerMessenger _messenger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstructionProcessJob" /> class.
    /// </summary>
    /// <param name="messenger">Service broadcasting cache notifications to registered servers.</param>
    /// <param name="logger">The typed logger.</param>
    /// <param name="globalSettings">The configuration for global settings.</param>
    public InstructionProcessJob(
        IServerMessenger messenger,
        ILogger<InstructionProcessJob> logger,
        IOptions<GlobalSettings> globalSettings)
    {
        _messenger = messenger;
        _logger = logger;

        Period = globalSettings.Value.DatabaseServerMessenger.TimeBetweenSyncOperations;
    }

    public Task RunJobAsync()
    {
        try
        {
            _messenger.Sync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed (will repeat).");
        }

        return Task.CompletedTask;
    }    
}
