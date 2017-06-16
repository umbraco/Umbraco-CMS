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

        public override async Task<bool> PerformRunAsync(CancellationToken token)
        {
            if (_appContext == null) return true; // repeat...

            switch (_appContext.GetCurrentServerRole())
            {
                case ServerRole.Slave:
                    LogHelper.Debug<ScheduledPublishing>("Does not run on slave servers.");
                    return true; // DO repeat, server role can change
                case ServerRole.Unknown:
                    LogHelper.Debug<ScheduledPublishing>("Does not run on servers with unknown role.");
                    return true; // DO repeat, server role can change
            }

            // ensure we do not run if not main domain, but do NOT lock it
            if (_appContext.MainDom.IsMainDom == false)
            {
                LogHelper.Debug<ScheduledPublishing>("Does not run if not MainDom.");
                return false; // do NOT repeat, going down
            }

            string umbracoAppUrl;
            try
            {
                umbracoAppUrl = _appContext == null || _appContext.UmbracoApplicationUrl.IsNullOrWhiteSpace()
                        ? null
                        : _appContext.UmbracoApplicationUrl;
                if (umbracoAppUrl.IsNullOrWhiteSpace())
                {
                    LogHelper.Warn<ScheduledPublishing>("No url for service (yet), skip.");
                    return true; // repeat
                }
            }
            catch (Exception e)
            {
                LogHelper.Error<ScheduledPublishing>("Could not acquire application url", e);
                return true; // repeat
            }

            var url = umbracoAppUrl + "/RestServices/ScheduledPublish/Index";

            using (DisposableTimer.DebugDuration<ScheduledPublishing>(
                () => string.Format("Scheduled publishing executing @ {0}", url),
                () => "Scheduled publishing complete"))
            {
                try
                {
                    using (var wc = new HttpClient())
                    {
                        var request = new HttpRequestMessage(HttpMethod.Post, url)
                        {
                            Content = new StringContent(string.Empty)
                        };

                        // running on a background task, requires its own (safe) scope
                        // (GetAuthenticationHeaderValue uses UserService to load the current user, hence requires a database)
                        // (might not need a scope but we don't know really)
                        using (var scope = ApplicationContext.Current.ScopeProvider.CreateScope())
                        {
                            //pass custom the authorization header
                            request.Headers.Authorization = AdminTokenAuthorizeAttribute.GetAuthenticationHeaderValue(_appContext);
                            scope.Complete();
                        }

                        var result = await wc.SendAsync(request, token);
                        var content = await result.Content.ReadAsStringAsync();

                        if (result.IsSuccessStatusCode)
                        {
                            LogHelper.Debug<ScheduledPublishing>(
                                () => string.Format(
                                    "Request successfully sent to url = \"{0}\". ", url));
                        }
                        else
                        {
                            var msg = string.Format(
                                    "Request failed with status code \"{0}\". Request content = \"{1}\".",
                                    result.StatusCode, content);
                            var ex = new HttpRequestException(msg);
                            LogHelper.Error<ScheduledPublishing>(msg, ex);
                        }
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
    }
}