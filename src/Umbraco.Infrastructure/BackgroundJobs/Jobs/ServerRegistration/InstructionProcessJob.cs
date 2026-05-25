// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs.ServerRegistration;

/// <summary>
///     Implements periodic database instruction processing as a hosted service.
/// </summary>
public class InstructionProcessJob : RecurringBackgroundJobBase
{
    /// <summary>
    /// Gets the delay time before the job is executed. The delay is fixed at one minute.
    /// </summary>
    public override TimeSpan Delay => TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets an array containing all possible values of the <see cref="ServerRole"/> enumeration.
    /// </summary>
    public override ServerRole[] ServerRoles => Enum.GetValues<ServerRole>();

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
        : base(globalSettings.Value.DatabaseServerMessenger.TimeBetweenSyncOperations)
    {
        _messenger = messenger;
        _logger = logger;
    }

    /// <summary>
    /// Executes the instruction processing job asynchronously by synchronizing messages using the messenger service.
    /// Logs an error if the synchronization fails, but always completes the task.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that is signaled when the host is shutting down.</param>
    /// <returns>
    /// A completed task representing the asynchronous operation.
    /// </returns>
    public override Task RunJobAsync(CancellationToken cancellationToken)
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
