using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;

namespace Umbraco.Web.Scheduling
{
    internal class KeepAlive : RecurringTaskBase
    {
        private readonly IRuntimeState _runtime;
        private readonly IKeepAliveSection _keepAliveSection;
        private readonly IProfilingLogger _logger;
        private static HttpClient _httpClient;

        public KeepAlive(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds,
            IRuntimeState runtime, IKeepAliveSection keepAliveSection, IProfilingLogger logger)
            : base(runner, delayMilliseconds, periodMilliseconds)
        {
            _runtime = runtime;
            _keepAliveSection = keepAliveSection;
            _logger = logger;
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

            using (_logger.DebugDuration<KeepAlive>("Keep alive executing", "Keep alive complete"))
            {
                var keepAlivePingUrl = _keepAliveSection.KeepAlivePingUrl;
                try
                {
                    if (keepAlivePingUrl.Contains("{umbracoApplicationUrl}"))
                    {
                        var umbracoAppUrl = _runtime.ApplicationUrl.ToString();
                        if (umbracoAppUrl.IsNullOrWhiteSpace())
                        {
                            _logger.Warn<KeepAlive>("No umbracoApplicationUrl for service (yet), skip.");
                            return true; // repeat
                        }

                        keepAlivePingUrl = keepAlivePingUrl.Replace("{umbracoApplicationUrl}", umbracoAppUrl.TrimEnd(Constants.CharArrays.ForwardSlash));
                    }

                    var request = new HttpRequestMessage(HttpMethod.Get, keepAlivePingUrl);
                    var result = await _httpClient.SendAsync(request, token);
                }
                catch (Exception ex)
                {
                    _logger.Error<KeepAlive, string>(ex, "Keep alive failed (at '{keepAlivePingUrl}').", keepAlivePingUrl);
                }
            }

            return true; // repeat
        }

        public override bool IsAsync => true;
    }
}
