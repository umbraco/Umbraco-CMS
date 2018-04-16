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
        private readonly ProfilingLogger _proflog;

        public KeepAlive(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds,
            IRuntimeState runtime, ILogger logger, ProfilingLogger proflog)
            : base(runner, delayMilliseconds, periodMilliseconds)
        {
            _runtime = runtime;
            _logger = logger;
            _proflog = proflog;
        }

        public override async Task<bool> PerformRunAsync(CancellationToken token)
        {
            // not on slaves nor unknown role servers
            switch (_runtime.ServerRole)
            {
                case ServerRole.Slave:
                    _logger.Debug<KeepAlive>("Does not run on slave servers.");
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

                    var url = umbracoAppUrl + "/ping.aspx";
                    using (var wc = new HttpClient())
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, url);
                        var result = await wc.SendAsync(request, token);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error<KeepAlive>(string.Format("Failed (at \"{0}\").", umbracoAppUrl), e);
                }
            }

            return true; // repeat
        }

        public override bool IsAsync => true;
    }
}
