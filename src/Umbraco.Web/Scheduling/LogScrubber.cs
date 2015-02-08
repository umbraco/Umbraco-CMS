using System;
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

        private int GetLogScrubbingMaximumAge(IUmbracoSettingsSection settings)
        {
            int maximumAge = 24 * 60 * 60;
            try
            {
                if (settings.Logging.MaxLogAge > -1)
                    maximumAge = settings.Logging.MaxLogAge;
            }
            catch (Exception e)
            {
                LogHelper.Error<Scheduler>("Unable to locate a log scrubbing maximum age.  Defaulting to 24 horus", e);
            }
            return maximumAge;

        }

        public static int GetLogScrubbingInterval(IUmbracoSettingsSection settings)
        {
            int interval = 24 * 60 * 60; //24 hours
            try
            {
                if (settings.Logging.CleaningMiliseconds > -1)
                    interval = settings.Logging.CleaningMiliseconds;
            }
            catch (Exception e)
            {
                LogHelper.Error<LogScrubber>("Unable to locate a log scrubbing interval.  Defaulting to 24 horus", e);
            }
            return interval;
        }

        public override void PerformRun()
        {
            using (DisposableTimer.DebugDuration<LogScrubber>(() => "Log scrubbing executing", () => "Log scrubbing complete"))
            {
                Log.CleanLogs(GetLogScrubbingMaximumAge(_settings));
            }           
        }

        public override Task PerformRunAsync()
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