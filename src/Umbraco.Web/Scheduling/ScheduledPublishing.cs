using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Umbraco.Web.Scheduling
{
    internal class ScheduledPublishing : RecurringTaskBase
    {
        private readonly IRuntimeState _runtime;
        private readonly IUserService _userService;
        private readonly IScopeProvider _scopeProvider;
        private readonly ILogger _logger;
        private readonly ProfilingLogger _proflog;

        public ScheduledPublishing(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds,
            IRuntimeState runtime, IUserService userService, IScopeProvider scopeProvider, ILogger logger, ProfilingLogger proflog)
            : base(runner, delayMilliseconds, periodMilliseconds)
        {
            _runtime = runtime;
            _userService = userService;
            _scopeProvider = scopeProvider;
            _logger = logger;
            _proflog = proflog;
        }

        public override async Task<bool> PerformRunAsync(CancellationToken token)
        {
            switch (_runtime.ServerRole)
            {
                case ServerRole.Slave:
                    _logger.Debug<ScheduledPublishing>("Does not run on slave servers.");
                    return true; // DO repeat, server role can change
                case ServerRole.Unknown:
                    _logger.Debug<ScheduledPublishing>("Does not run on servers with unknown role.");
                    return true; // DO repeat, server role can change
            }

            // ensure we do not run if not main domain, but do NOT lock it
            if (_runtime.IsMainDom == false)
            {
                _logger.Debug<ScheduledPublishing>("Does not run if not MainDom.");
                return false; // do NOT repeat, going down
            }

            UmbracoContext tempContext = null;
            try
            {
                // DO not run publishing if content is re-loading
                if (content.Instance.isInitializing == false)
                {
                    //TODO: We should remove this in v8, this is a backwards compat hack
                    // see notes in CacheRefresherEventHandler
                    // because notifications will not be sent if there is no UmbracoContext
                    // see NotificationServiceExtensions
                    var httpContext = new HttpContextWrapper(new HttpContext(new SimpleWorkerRequest("temp.aspx", "", new StringWriter())));
                    tempContext = UmbracoContext.EnsureContext(
                        httpContext,
                        _appContext,
                        new WebSecurity(httpContext, _appContext),
                        _settings,
                        UrlProviderResolver.Current.Providers,
                        true);

                    var publisher = new ScheduledPublisher(_appContext.Services.ContentService);
                    var count = publisher.CheckPendingAndProcess();
                    _logger.Warn<ScheduledPublishing>("No url for service (yet), skip.");
                }
            }
            catch (Exception e)
            {
                _logger.Error<ScheduledPublishing>("Could not acquire application url", e);
            }
            finally
            {
                if (tempContext != null)
                    tempContext.Dispose(); // nulls the ThreadStatic context
            }

            return true; // repeat
        }

        public override bool IsAsync => true;
    }
}
