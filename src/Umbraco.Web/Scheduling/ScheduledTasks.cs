using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Publishing;
using Umbraco.Core.Sync;
using Umbraco.Core;

namespace Umbraco.Web.Scheduling
{
    //TODO: No scheduled task (i.e. URL) would be secured, so if people are actually using these each task
    // would need to be a publicly available task (URL) which isn't really very good :(
    // We should really be using the AdminTokenAuthorizeAttribute for this stuff

    internal class ScheduledTasks : DelayedRecurringTaskBase<ScheduledTasks>
    {
        private readonly ApplicationContext _appContext;
        private readonly IUmbracoSettingsSection _settings;
        private readonly Func<CancellationToken> _cancellationToken;
        private static readonly Hashtable ScheduledTaskTimes = new Hashtable();
        private static bool _isPublishingRunning = false;

        public ScheduledTasks(IBackgroundTaskRunner<ScheduledTasks> runner, int delayMilliseconds, int periodMilliseconds, 
            ApplicationContext appContext, IUmbracoSettingsSection settings, Func<CancellationToken> cancellationToken)
            : base(runner, delayMilliseconds, periodMilliseconds)
        {
            _appContext = appContext;
            _settings = settings;
            _cancellationToken = cancellationToken;
        }

        public ScheduledTasks(ScheduledTasks source)
            : base(source)
        {
            _appContext = source._appContext;
            _settings = source._settings;
        }

        protected override ScheduledTasks GetRecurring()
        {
            return new ScheduledTasks(this);
        }

        private async Task ProcessTasksAsync()
        {
            var scheduledTasks = _settings.ScheduledTasks.Tasks;
            foreach (var t in scheduledTasks)
            {
                var runTask = false;
                if (ScheduledTaskTimes.ContainsKey(t.Alias) == false)
                {
                    runTask = true;
                    ScheduledTaskTimes.Add(t.Alias, DateTime.Now);
                }

                // Add 1 second to timespan to compensate for differencies in timer
                else if (
                    new TimeSpan(
                        DateTime.Now.Ticks - ((DateTime)ScheduledTaskTimes[t.Alias]).Ticks).TotalSeconds + 1 >= t.Interval)
                {
                    runTask = true;
                    ScheduledTaskTimes[t.Alias] = DateTime.Now;
                }

                if (runTask)
                {
                    bool taskResult = await GetTaskByHttpAync(t.Url);
                    if (t.Log)
                        LogHelper.Info<ScheduledTasks>(string.Format("{0} has been called with response: {1}", t.Alias, taskResult));
                }
            }
        }

        private async Task<bool> GetTaskByHttpAync(string url)
        {
            using (var wc = new HttpClient())
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Get,
                    Content = new StringContent(string.Empty)
                };
                //TODO: pass custom the authorization header, currently these aren't really secured!
                //request.Headers.Authorization = AdminTokenAuthorizeAttribute.GetAuthenticationHeaderValue(_appContext);

                var token = new CancellationToken();
                try
                {
                    token = _cancellationToken();
                }
                catch (InvalidOperationException)
                {
                    //There is no valid token, so we'll continue with the empty one
                }

                try
                {
                    var result = await wc.SendAsync(request, token);
                    return result.StatusCode == HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    LogHelper.Error<ScheduledTasks>("An error occurred calling web task for url: " + url, ex);
                }
                return false;
            }
        }

        public override void PerformRun()
        {
            throw new NotImplementedException();
        }

        public override async Task PerformRunAsync()
        {
            if (ServerEnvironmentHelper.GetStatus(_settings) == CurrentServerEnvironmentStatus.Slave)
            {
                LogHelper.Debug<ScheduledTasks>("Does not run on slave servers.");
                return;
            }

            using (DisposableTimer.DebugDuration<ScheduledTasks>(() => "Scheduled tasks executing", () => "Scheduled tasks complete"))
            {
                if (_isPublishingRunning) return;

                _isPublishingRunning = true;

                try
                {
                    await ProcessTasksAsync();
                }
                catch (Exception ee)
                {
                    LogHelper.Error<ScheduledTasks>("Error executing scheduled task", ee);
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