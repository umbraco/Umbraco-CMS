using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Scheduling
{
    internal class ScheduledPublishing : RecurringTaskBase
    {
        private readonly ApplicationContext _appContext;
        private readonly IUmbracoSettingsSection _settings;

        public ScheduledPublishing(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds,
            ApplicationContext appContext, IUmbracoSettingsSection settings)
            : base(runner, delayMilliseconds, periodMilliseconds)
        {
            _appContext = appContext;
            _settings = settings;
        }

        public override bool PerformRun()
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> PerformRunAsync(CancellationToken token)
        {            
            if (_appContext == null) return true; // repeat...

            if (ServerEnvironmentHelper.GetStatus(_settings) == CurrentServerEnvironmentStatus.Slave)
            {
                LogHelper.Debug<ScheduledPublishing>("Does not run on slave servers.");
                return false; // do NOT repeat, server status comes from config and will NOT change
            }

            using (DisposableTimer.DebugDuration<ScheduledPublishing>(() => "Scheduled publishing executing", () => "Scheduled publishing complete"))
            {
                string umbracoAppUrl = null;

                try
                {
                    umbracoAppUrl = _appContext.UmbracoApplicationUrl;
                    if (umbracoAppUrl.IsNullOrWhiteSpace())
                    {
                        LogHelper.Warn<ScheduledPublishing>("No url for service (yet), skip.");
                        return true; // repeat
                    }

                    var url = umbracoAppUrl + "/RestServices/ScheduledPublish/Index";
                    using (var wc = new HttpClient())
                    {
                        var request = new HttpRequestMessage()
                        {
                            RequestUri = new Uri(url),
                            Method = HttpMethod.Post,
                            Content = new StringContent(string.Empty)
                        };
                        //pass custom the authorization header
                        request.Headers.Authorization = AdminTokenAuthorizeAttribute.GetAuthenticationHeaderValue(_appContext);

                        var result = await wc.SendAsync(request, token);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error<ScheduledPublishing>(string.Format("Failed (at \"{0}\").", umbracoAppUrl), e);
                }
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