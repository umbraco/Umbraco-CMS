// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Web;

namespace Umbraco.Cms.Infrastructure.HostedServices
{
    /// <summary>
    /// Hosted service implementation for scheduled publishing feature.
    /// </summary>
    /// <remarks>
    /// Runs only on non-replica servers.</remarks>
    public class ScheduledPublishing : RecurringHostedServiceBase
    {
        private readonly IContentService _contentService;
        private readonly ILogger<ScheduledPublishing> _logger;
        private readonly IMainDom _mainDom;
        private readonly IRuntimeState _runtimeState;
        private readonly IServerMessenger _serverMessenger;
        private readonly IBackOfficeSecurityFactory _backofficeSecurityFactory;
        private readonly IServerRoleAccessor _serverRegistrar;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledPublishing"/> class.
        /// </summary>
        /// <param name="runtimeState">Representation of the state of the Umbraco runtime.</param>
        /// <param name="mainDom">Representation of the main application domain.</param>
        /// <param name="serverRegistrar">Provider of server registrations to the distributed cache.</param>
        /// <param name="contentService">Service for handling content operations.</param>
        /// <param name="umbracoContextFactory">Service for creating and managing Umbraco context.</param>
        /// <param name="logger">The typed logger.</param>
        /// <param name="serverMessenger">Service broadcasting cache notifications to registered servers.</param>
        /// <param name="backofficeSecurityFactory">Creates and manages <see cref="IBackOfficeSecurity"/> instances.</param>
        public ScheduledPublishing(
            IRuntimeState runtimeState,
            IMainDom mainDom,
            IServerRoleAccessor serverRegistrar,
            IContentService contentService,
            IUmbracoContextFactory umbracoContextFactory,
            ILogger<ScheduledPublishing> logger,
            IServerMessenger serverMessenger,
            IBackOfficeSecurityFactory backofficeSecurityFactory)
            : base(TimeSpan.FromMinutes(1), DefaultDelay)
        {
            _runtimeState = runtimeState;
            _mainDom = mainDom;
            _serverRegistrar = serverRegistrar;
            _contentService = contentService;
            _umbracoContextFactory = umbracoContextFactory;
            _logger = logger;
            _serverMessenger = serverMessenger;
            _backofficeSecurityFactory = backofficeSecurityFactory;
        }

        internal override Task PerformExecuteAsync(object state)
        {
            if (Suspendable.ScheduledPublishing.CanRun == false)
            {
                return Task.CompletedTask;
            }

            switch (_serverRegistrar.CurrentServerRole)
            {
                case ServerRole.Replica:
                    _logger.LogDebug("Does not run on replica servers.");
                    return Task.CompletedTask;
                case ServerRole.Unknown:
                    _logger.LogDebug("Does not run on servers with unknown role.");
                    return Task.CompletedTask;
            }

            // Ensure we do not run if not main domain, but do NOT lock it
            if (_mainDom.IsMainDom == false)
            {
                _logger.LogDebug("Does not run if not MainDom.");
                return Task.CompletedTask;
            }

            // Do NOT run publishing if not properly running
            if (_runtimeState.Level != RuntimeLevel.Run)
            {
                _logger.LogDebug("Does not run if run level is not Run.");
                return Task.CompletedTask;
            }

            try
            {
                // We don't need an explicit scope here because PerformScheduledPublish creates it's own scope
                // so it's safe as it will create it's own ambient scope.
                // Ensure we run with an UmbracoContext, because this will run in a background task,
                // and developers may be using the UmbracoContext in the event handlers.

                // TODO: or maybe not, CacheRefresherComponent already ensures a context when handling events
                // - UmbracoContext 'current' needs to be refactored and cleaned up
                // - batched messenger should not depend on a current HttpContext
                //    but then what should be its "scope"? could we attach it to scopes?
                // - and we should definitively *not* have to flush it here (should be auto)
                //
                _backofficeSecurityFactory.EnsureBackOfficeSecurity();
                using UmbracoContextReference contextReference = _umbracoContextFactory.EnsureUmbracoContext();
                try
                {
                    // Run
                    IEnumerable<PublishResult> result = _contentService.PerformScheduledPublish(DateTime.Now);
                    foreach (IGrouping<PublishResultType, PublishResult> grouped in result.GroupBy(x => x.Result))
                    {
                        _logger.LogInformation(
                            "Scheduled publishing result: '{StatusCount}' items with status {Status}",
                            grouped.Count(),
                            grouped.Key);
                    }
                }
                finally
                {
                    // If running on a temp context, we have to flush the messenger
                    if (contextReference.IsRoot)
                    {
                        _serverMessenger.SendMessages();
                    }
                }
            }
            catch (Exception ex)
            {
                // important to catch *everything* to ensure the task repeats
                _logger.LogError(ex, "Failed.");
            }

            return Task.CompletedTask;
        }
    }
}
