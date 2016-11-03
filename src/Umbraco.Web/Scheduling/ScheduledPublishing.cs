using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Scheduling
{
    internal class ScheduledPublishing : RecurringTaskBase
    {
        private readonly IRuntimeState _runtime;
        private readonly IUserService _userService;
        private readonly ILogger _logger;
        private readonly ProfilingLogger _proflog;

        public ScheduledPublishing(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds,
            IRuntimeState runtime, IUserService userService, ILogger logger, ProfilingLogger proflog)
            : base(runner, delayMilliseconds, periodMilliseconds)
        {
            _runtime = runtime;
            _userService = userService;
            _logger = logger;
            _proflog = proflog;
        }

        public override bool PerformRun()
        {
            throw new NotImplementedException();
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

            string umbracoAppUrl;
            try
            {
                umbracoAppUrl = _runtime.ApplicationUrl.ToString();
                if (umbracoAppUrl.IsNullOrWhiteSpace())
                {
                    _logger.Warn<ScheduledPublishing>("No url for service (yet), skip.");
                    return true; // repeat
                }
            }
            catch (Exception e)
            {
                _logger.Error<ScheduledPublishing>("Could not acquire application url", e);
                return true; // repeat
            }

            var url = umbracoAppUrl + "/RestServices/ScheduledPublish/Index";

            using (_proflog.DebugDuration<ScheduledPublishing>($"Scheduled publishing executing at {url}",  "Scheduled publishing complete"))
            {
                try
                {
                    using (var wc = new HttpClient())
                    {
                        var request = new HttpRequestMessage(HttpMethod.Post, url)
                        {
                            Content = new StringContent(string.Empty)
                        };
                        //pass custom the authorization header
                        request.Headers.Authorization = AdminTokenAuthorizeAttribute.GetAuthenticationHeaderValue(_userService);

                        var result = await wc.SendAsync(request, token);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error<ScheduledPublishing>($"Failed (at \"{umbracoAppUrl}\").", e);
                }
            }

            return true; // repeat
        }

        public override bool IsAsync => true;

        public override bool RunsOnShutdown => false;
    }
}