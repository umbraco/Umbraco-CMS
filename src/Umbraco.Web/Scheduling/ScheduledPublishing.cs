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
    internal class ScheduledPublishing : DelayedRecurringTaskBase<ScheduledPublishing>
    {
        private readonly ApplicationContext _appContext;
        private readonly IUmbracoSettingsSection _settings;

        private static bool _isPublishingRunning;

        public ScheduledPublishing(IBackgroundTaskRunner<ScheduledPublishing> runner, int delayMilliseconds, int periodMilliseconds,
            ApplicationContext appContext, IUmbracoSettingsSection settings)
            : base(runner, delayMilliseconds, periodMilliseconds)
        {
            _appContext = appContext;
            _settings = settings;
        }

        private ScheduledPublishing(ScheduledPublishing source)
            : base(source)
        {
            _appContext = source._appContext;
            _settings = source._settings;
        }

        protected override ScheduledPublishing GetRecurring()
        {
            return new ScheduledPublishing(this);
        }

        public override void PerformRun()
        {
            throw new NotImplementedException();
        }

        public override async Task PerformRunAsync(CancellationToken token)
        {
            
            if (_appContext == null) return;
            if (ServerEnvironmentHelper.GetStatus(_settings) == CurrentServerEnvironmentStatus.Slave)
            {
                LogHelper.Debug<ScheduledPublishing>("Does not run on slave servers.");
                return;
            }

            using (DisposableTimer.DebugDuration<ScheduledPublishing>(() => "Scheduled publishing executing", () => "Scheduled publishing complete"))
            {
                if (_isPublishingRunning) return;

                _isPublishingRunning = true;

                var umbracoBaseUrl = ServerEnvironmentHelper.GetCurrentServerUmbracoBaseUrl(_appContext, _settings);

                try
                {

                    if (string.IsNullOrWhiteSpace(umbracoBaseUrl))
                    {
                        LogHelper.Warn<ScheduledPublishing>("No url for service (yet), skip.");
                    }
                    else
                    {
                        var url = string.Format("{0}RestServices/ScheduledPublish/Index", umbracoBaseUrl.EnsureEndsWith('/'));
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

                            try
                            {
                                var result = await wc.SendAsync(request, token);
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error<ScheduledPublishing>("An error occurred calling scheduled publish url", ex);
                            }
                        }
                    }
                }
                catch (Exception ee)
                {
                    LogHelper.Error<ScheduledPublishing>(
                        string.Format("An error occurred with the scheduled publishing. The base url used in the request was: {0}, see http://our.umbraco.org/documentation/Using-Umbraco/Config-files/umbracoSettings/#ScheduledTasks documentation for details on setting a baseUrl if this is in error", umbracoBaseUrl)
                        , ee);
                }
                finally
                {
                    _isPublishingRunning = false;
                }
            }            
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