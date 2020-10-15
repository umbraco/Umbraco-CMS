using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;
using Microsoft.Extensions.Logging;

namespace Umbraco.Web.Scheduling
{
    public class KeepAlive : RecurringTaskBase
    {
        private readonly IRequestAccessor _requestAccessor;
        private readonly IMainDom _mainDom;
        private readonly KeepAliveSettings _keepAliveSettings;
        private readonly ILogger<KeepAlive> _logger;
        private readonly IProfilingLogger _profilingLogger;
        private readonly IServerRegistrar _serverRegistrar;
        private static HttpClient _httpClient;

        public KeepAlive(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds,
            IRequestAccessor requestAccessor, IMainDom mainDom, IOptions<KeepAliveSettings> keepAliveSettings, ILogger<KeepAlive> logger, IProfilingLogger profilingLogger, IServerRegistrar serverRegistrar)
            : base(runner, delayMilliseconds, periodMilliseconds)
        {
            _requestAccessor = requestAccessor;
            _mainDom = mainDom;
            _keepAliveSettings = keepAliveSettings.Value;
            _logger = logger;
            _profilingLogger = profilingLogger;
            _serverRegistrar = serverRegistrar;
            if (_httpClient == null)
            {
                _httpClient = new HttpClient();
            }
        }

        public override async Task<bool> PerformRunAsync(CancellationToken token)
        {
            // not on replicas nor unknown role servers
            switch (_serverRegistrar.GetCurrentServerRole())
            {
                case ServerRole.Replica:
                    _logger.LogDebug("Does not run on replica servers.");
                    return true; // role may change!
                case ServerRole.Unknown:
                    _logger.LogDebug("Does not run on servers with unknown role.");
                    return true; // role may change!
            }

            // ensure we do not run if not main domain, but do NOT lock it
            if (_mainDom.IsMainDom == false)
            {
                _logger.LogDebug("Does not run if not MainDom.");
                return false; // do NOT repeat, going down
            }

            using (_profilingLogger.DebugDuration<KeepAlive>("Keep alive executing", "Keep alive complete"))
            {
                var keepAlivePingUrl = _keepAliveSettings.KeepAlivePingUrl;
                try
                {
                    if (keepAlivePingUrl.Contains("{umbracoApplicationUrl}"))
                    {
                        var umbracoAppUrl = _requestAccessor.GetApplicationUrl().ToString();
                        if (umbracoAppUrl.IsNullOrWhiteSpace())
                        {
                            _logger.LogWarning("No umbracoApplicationUrl for service (yet), skip.");
                            return true; // repeat
                        }

                        keepAlivePingUrl = keepAlivePingUrl.Replace("{umbracoApplicationUrl}", umbracoAppUrl.TrimEnd('/'));
                    }

                    var request = new HttpRequestMessage(HttpMethod.Get, keepAlivePingUrl);
                    var result = await _httpClient.SendAsync(request, token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Keep alive failed (at '{keepAlivePingUrl}').", keepAlivePingUrl);
                }
            }

            return true; // repeat
        }

        public override bool IsAsync => true;
    }
}
