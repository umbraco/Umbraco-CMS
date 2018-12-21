using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;

namespace Umbraco.Web.Scheduling
{
    internal class KeepAlive : RecurringTaskBase
    {
        private readonly IRuntimeState _runtime;
        private readonly ILogger _logger;
        private static HttpClient _httpClient;
        private readonly ProfilingLogger _proflog;

        public KeepAlive(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds,
            IRuntimeState runtime, ILogger logger, ProfilingLogger proflog)
            : base(runner, delayMilliseconds, periodMilliseconds)
        {
            _runtime = runtime;
            _logger = logger;
            _proflog = proflog;
            if (_httpClient == null)
                _httpClient = new HttpClient();
        }

        public override async Task<bool> PerformRunAsync(CancellationToken token)
        {
            // not on replicas nor unknown role servers
            switch (_runtime.ServerRole)
            {
                case ServerRole.Replica:
                    _logger.Debug<KeepAlive>("Does not run on replica servers.");
                    return true; // role may change!
                case ServerRole.Unknown:
                    _logger.Debug<KeepAlive>("Does not run on servers with unknown role.");
                    return true; // role may change!
            }

            // ensure we do not run if not main domain, but do NOT lock it
            if (_runtime.IsMainDom == false)
            {
                _logger.Debug<KeepAlive>("Does not run if not MainDom.");
                return false; // do NOT repeat, going down
            }

            using (_proflog.DebugDuration<KeepAlive>("Keep alive executing", "Keep alive complete"))
            {
                string umbracoAppUrl = null;

                try
                {
                    umbracoAppUrl = _runtime.ApplicationUrl.ToString();
                    if (umbracoAppUrl.IsNullOrWhiteSpace())
                    {
                        _logger.Warn<KeepAlive>("No url for service (yet), skip.");
                        return true; // repeat
                    }

                    var url = umbracoAppUrl.TrimEnd('/') + "/api/keepalive/ping";

                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    var result = await _httpClient.SendAsync(request, token);
                }
                catch (Exception ex)
                {
                    _logger.Error<KeepAlive>(ex, "Failed (at '{UmbracoAppUrl}').", umbracoAppUrl);
                }
            }

            return true; // repeat
        }

        public override bool IsAsync => true;
    }
}
