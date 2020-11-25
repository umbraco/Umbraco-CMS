using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Umbraco.Infrastructure.HostedServices.ServerRegistration
{
    /// <summary>
    /// Implements periodic server "touching" (to mark as active/deactive) as a hosted service.
    /// </summary>
    [UmbracoVolatile]
    public class TouchServerTask : RecurringHostedServiceBase
    {
        private readonly IRuntimeState _runtimeState;
        private readonly IServerRegistrationService _serverRegistrationService;
        private readonly IRequestAccessor _requestAccessor;
        private readonly ILogger<TouchServerTask> _logger;
        private GlobalSettings _globalSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchServerTask"/> class.
        /// </summary>
        public TouchServerTask(IRuntimeState runtimeState, IServerRegistrationService serverRegistrationService, IRequestAccessor requestAccessor, ILogger<TouchServerTask> logger, IOptions<GlobalSettings> globalSettings)
            : base(globalSettings.Value.DatabaseServerRegistrar.WaitTimeBetweenCalls, TimeSpan.FromSeconds(15))
        {
            _runtimeState = runtimeState;
            _serverRegistrationService = serverRegistrationService ?? throw new ArgumentNullException(nameof(serverRegistrationService));
            _requestAccessor = requestAccessor;
            _logger = logger;
            _globalSettings = globalSettings.Value;
        }

        internal override async Task PerformExecuteAsync(object state)
        {
            if (_runtimeState.Level != RuntimeLevel.Run)
            {
                return;
            }

            var serverAddress = _requestAccessor.GetApplicationUrl()?.ToString();
            if (serverAddress.IsNullOrWhiteSpace())
            {
                _logger.LogWarning("No umbracoApplicationUrl for service (yet), skip.");
                return;
            }

            try
            {
                // TouchServer uses a proper unit of work etc underneath so even in a
                // background task it is safe to call it without dealing with any scope.
                _serverRegistrationService.TouchServer(serverAddress, _serverRegistrationService.CurrentServerIdentity, _globalSettings.DatabaseServerRegistrar.StaleServerTimeout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update server record in database.");
            }
        }
    }
}
