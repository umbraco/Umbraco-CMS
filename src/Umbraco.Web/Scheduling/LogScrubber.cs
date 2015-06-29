using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Scheduling
{
    internal class LogScrubber : DelayedRecurringTaskBase<LogScrubber>
    {
        private readonly ApplicationContext _appContext;
        private readonly IUmbracoSettingsSection _settings;

        public LogScrubber(IBackgroundTaskRunner<LogScrubber> runner, int delayMilliseconds, int periodMilliseconds, 
            ApplicationContext appContext, IUmbracoSettingsSection settings)
            : base(runner, delayMilliseconds, periodMilliseconds)
        {
            _appContext = appContext;
            _settings = settings;
        }

        public LogScrubber(LogScrubber source)
            : base(source)
        {
            _appContext = source._appContext;
            _settings = source._settings;
        }

        protected override LogScrubber GetRecurring()
        {
            return new LogScrubber(this);
        }

        // maximum age, in minutes
        private int GetLogScrubbingMaximumAge(IUmbracoSettingsSection settings)
        {
            var maximumAge = 24 * 60; // 24 hours, in minutes
            try
            {
                if (settings.Logging.MaxLogAge > -1)
                    maximumAge = settings.Logging.MaxLogAge;
            }
            catch (Exception e)
            {
                LogHelper.Error<Scheduler>("Unable to locate a log scrubbing maximum age. Defaulting to 24 hours.", e);
            }
            return maximumAge;

        }

        public static int GetLogScrubbingInterval(IUmbracoSettingsSection settings)
        {
            var interval = 4 * 60 * 60 * 1000; // 4 hours, in milliseconds
            try
            {
                if (settings.Logging.CleaningMiliseconds > -1)
                    interval = settings.Logging.CleaningMiliseconds;
            }
            catch (Exception e)
            {
                LogHelper.Error<LogScrubber>("Unable to locate a log scrubbing interval. Defaulting to 4 hours.", e);
            }
            return interval;
        }

        public override void PerformRun()
        {
            using (DisposableTimer.DebugDuration<LogScrubber>("Log scrubbing executing", "Log scrubbing complete"))
            {
                Log.CleanLogs(GetLogScrubbingMaximumAge(_settings));
            }           
        }

        public override Task PerformRunAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override bool IsAsync
        {
            get { return false; }
        }

        public override bool RunsOnShutdown
        {
            get { return false; }
        }
    }
}