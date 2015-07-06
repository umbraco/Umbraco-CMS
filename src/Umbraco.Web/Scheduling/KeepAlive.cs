using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;

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

        public override bool PerformRun()
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> PerformRunAsync(CancellationToken token)
        {
            if (_appContext == null) return true; // repeat...

            string umbracoAppUrl = null;

            try
            {
                using (DisposableTimer.DebugDuration<KeepAlive>(() => "Keep alive executing", () => "Keep alive complete"))
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
                        var request = new HttpRequestMessage()
                        {
                            RequestUri = new Uri(url),
                            Method = HttpMethod.Get
                        };

                        var result = await wc.SendAsync(request, token);
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error<KeepAlive>(string.Format("Failed (at \"{0}\").", umbracoAppUrl), e);
            }

            return true; // repeat
        }

        public override bool IsAsync
        {
            get { return true; }
        }

        public override bool RunsOnShutdown
        {
            get { return false; }
        }
    }
}