// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.HostedServices.ServerRegistration
{
    /// <summary>
    /// Implements periodic database instruction processing as a hosted service.
    /// </summary>
    public class InstructionProcessTask : RecurringHostedServiceBase
    {
        private readonly IRuntimeState _runtimeState;
        private readonly IServerMessenger _messenger;
        private readonly ILogger<InstructionProcessTask> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstructionProcessTask"/> class.
        /// </summary>
        /// <param name="runtimeState">Representation of the state of the Umbraco runtime.</param>
        /// <param name="messenger">Service broadcasting cache notifications to registered servers.</param>
        /// <param name="logger">The typed logger.</param>
        /// <param name="globalSettings">The configuration for global settings.</param>
        public InstructionProcessTask(IRuntimeState runtimeState, IServerMessenger messenger, ILogger<InstructionProcessTask> logger, IOptions<GlobalSettings> globalSettings)
            : base(globalSettings.Value.DatabaseServerMessenger.TimeBetweenSyncOperations, TimeSpan.FromMinutes(1))
        {
            _runtimeState = runtimeState;
            _messenger = messenger;
            _logger = logger;
        }

        public override Task PerformExecuteAsync(object state)
        {
            if (_runtimeState.Level != RuntimeLevel.Run)
            {
                return Task.CompletedTask;
            }

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
}
