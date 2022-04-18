// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HostedServices.ServerRegistration
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
        private readonly IServerRoleAccessor _serverRoleAccessor;
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
            IOptions<GlobalSettings> globalSettings,
            IServerRoleAccessor serverRoleAccessor)
            : base(logger, globalSettings.Value.DatabaseServerRegistrar.WaitTimeBetweenCalls, TimeSpan.FromSeconds(15))
        {
            _runtimeState = runtimeState;
            _serverRegistrationService = serverRegistrationService ?? throw new ArgumentNullException(nameof(serverRegistrationService));
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
            _globalSettings = globalSettings.Value;
            _serverRoleAccessor = serverRoleAccessor;
        }

        [Obsolete("Use constructor that takes an IServerRoleAccessor")]
        public TouchServerTask(
            IRuntimeState runtimeState,
            IServerRegistrationService serverRegistrationService,
            IHostingEnvironment hostingEnvironment,
            ILogger<TouchServerTask> logger,
            IOptions<GlobalSettings> globalSettings)
            : this(
                runtimeState,
                serverRegistrationService,
                hostingEnvironment,
                logger,
                globalSettings,
                StaticServiceProvider.Instance.GetRequiredService<IServerRoleAccessor>())
        {
        }

        public override Task PerformExecuteAsync(object state)
        {
            if (_runtimeState.Level != RuntimeLevel.Run)
            {
                return Task.CompletedTask;
            }

            // If the IServerRoleAccessor has been changed away from ElectedServerRoleAccessor this task no longer makes sense,
            // since all it's used for is to allow the ElectedServerRoleAccessor
            // to figure out what role a given server has, so we just stop this task.
            if (_serverRoleAccessor is not ElectedServerRoleAccessor)
            {
                return StopAsync(CancellationToken.None);
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
