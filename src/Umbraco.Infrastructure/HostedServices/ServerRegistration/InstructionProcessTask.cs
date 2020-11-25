using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Sync;

namespace Umbraco.Infrastructure.HostedServices.ServerRegistration
{
    /// <summary>
    /// Implements periodic database instruction processing as a hosted service.
    /// </summary>
    [UmbracoVolatile]
    public class InstructionProcessTask : RecurringHostedServiceBase
    {
        private readonly IRuntimeState _runtimeState;
        private readonly IDatabaseServerMessenger _messenger;
        private readonly ILogger<InstructionProcessTask> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstructionProcessTask"/> class.
        /// </summary>
        public InstructionProcessTask(IRuntimeState runtimeState, IServerMessenger messenger, ILogger<InstructionProcessTask> logger, IOptions<GlobalSettings> globalSettings)
            : base(globalSettings.Value.DatabaseServerMessenger.TimeBetweenSyncOperations, TimeSpan.FromMinutes(1))
        {
            _runtimeState = runtimeState;
            _messenger = messenger as IDatabaseServerMessenger ?? throw new ArgumentNullException(nameof(messenger));
            _logger = logger;
        }

        internal override async Task PerformExecuteAsync(object state)
        {
            if (_runtimeState.Level != RuntimeLevel.Run)
            {
                return;
            }

            try
            {
                _messenger.Sync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed (will repeat).");
            }
        }
    }
}
