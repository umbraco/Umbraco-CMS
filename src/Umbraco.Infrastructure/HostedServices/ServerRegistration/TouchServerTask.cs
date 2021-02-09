// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.Services;

namespace Umbraco.Infrastructure.HostedServices.ServerRegistration
{
    /// <summary>
    /// Implements periodic server "touching" (to mark as active/deactive) as a hosted service.
    /// </summary>
    public class TouchServerTask : RecurringHostedServiceBase
    {
        private readonly IRuntimeState _runtimeState;
        private readonly IServerRegistrationService _serverRegistrationService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger<TouchServerTask> _logger;
        private readonly GlobalSettings _globalSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchServerTask"/> class.
        /// </summary>
        /// <param name="runtimeState">Representation of the state of the Umbraco runtime.</param>
        /// <param name="serverRegistrationService">Services for server registrations.</param>
        /// <param name="requestAccessor">Accessor for the current request.</param>
        /// <param name="logger">The typed logger.</param>
        /// <param name="globalSettings">The configuration for global settings.</param>
        public TouchServerTask(
            IRuntimeState runtimeState,
            IServerRegistrationService serverRegistrationService,
            IHostingEnvironment hostingEnvironment,
            ILogger<TouchServerTask> logger,
            IOptions<GlobalSettings> globalSettings)
            : base(globalSettings.Value.DatabaseServerRegistrar.WaitTimeBetweenCalls, TimeSpan.FromSeconds(15))
        {
            _runtimeState = runtimeState;
            _serverRegistrationService = serverRegistrationService ?? throw new ArgumentNullException(nameof(serverRegistrationService));
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
            _globalSettings = globalSettings.Value;
        }

        internal override Task PerformExecuteAsync(object state)
        {
            if (_runtimeState.Level != RuntimeLevel.Run)
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
                _serverRegistrationService.TouchServer(serverAddress, _globalSettings.DatabaseServerRegistrar.StaleServerTimeout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update server record in database.");
            }

            return Task.CompletedTask;
        }
    }
}
