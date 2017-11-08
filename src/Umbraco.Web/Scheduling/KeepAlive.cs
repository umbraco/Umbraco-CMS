using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;

namespace Umbraco.Web.Scheduling
{
    internal class KeepAlive : RecurringTaskBase
    {
        private readonly ApplicationContext _appContext;

        public KeepAlive(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds, 
            ApplicationContext appContext)
            : base(runner, delayMilliseconds, periodMilliseconds)
        {
            _appContext = appContext;
        }

        private ILogger Logger { get { return _appContext.ProfilingLogger.Logger; } }

        public override async Task<bool> PerformRunAsync(CancellationToken token)
        {
            if (_appContext == null) return true; // repeat...
            
            switch (_appContext.GetCurrentServerRole())
            {
                case ServerRole.Slave:
                    Logger.Debug<ScheduledPublishing>("Does not run on slave servers.");
                    return true; // DO repeat, server role can change
                case ServerRole.Unknown:
                    Logger.Debug<ScheduledPublishing>("Does not run on servers with unknown role.");
                    return true; // DO repeat, server role can change
            }

            // ensure we do not run if not main domain, but do NOT lock it
            if (_appContext.MainDom.IsMainDom == false)
            {
                LogHelper.Debug<KeepAlive>("Does not run if not MainDom.");
                return false; // do NOT repeat, going down
            }

            using (_appContext.ProfilingLogger.DebugDuration<KeepAlive>("Keep alive executing", "Keep alive complete"))
            {
                string umbracoAppUrl = null;

                try
                {
                    umbracoAppUrl = _appContext.UmbracoApplicationUrl;
                    if (umbracoAppUrl.IsNullOrWhiteSpace())
                    {
                        LogHelper.Warn<KeepAlive>("No url for service (yet), skip.");
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
                    LogHelper.Error<KeepAlive>(string.Format("Failed (at \"{0}\").", umbracoAppUrl), e);
                }
            }

            return true; // repeat
        }

        public override bool IsAsync
        {
            get { return true; }
        }
    }
}