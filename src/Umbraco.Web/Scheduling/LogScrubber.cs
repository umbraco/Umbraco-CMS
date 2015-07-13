using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;

namespace Umbraco.Web.Scheduling
{
    internal class LogScrubber : RecurringTaskBase
    {
        private readonly ApplicationContext _appContext;
        private readonly IUmbracoSettingsSection _settings;

        public LogScrubber(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds, 
            ApplicationContext appContext, IUmbracoSettingsSection settings)
            : base(runner, delayMilliseconds, periodMilliseconds)
        {
            _appContext = appContext;
            _settings = settings;
        }

        // maximum age, in minutes
        private static int GetLogScrubbingMaximumAge(IUmbracoSettingsSection settings)
        {
            var maximumAge = 24 * 60; // 24 hours, in minutes
            try
            {
                if (settings.Logging.MaxLogAge > -1)
                    maximumAge = settings.Logging.MaxLogAge;
            }
            catch (Exception e)
            {
                LogHelper.Error<LogScrubber>("Unable to locate a log scrubbing maximum age. Defaulting to 24 hours.", e);
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

        public override bool PerformRun()
        {
            if (_appContext == null) return true; // repeat...

            if (ServerEnvironmentHelper.GetStatus(_settings) == CurrentServerEnvironmentStatus.Slave)
            {
                LogHelper.Debug<LogScrubber>("Does not run on slave servers.");
                return false; // do NOT repeat, server status comes from config and will NOT change
            }

            using (DisposableTimer.DebugDuration<LogScrubber>("Log scrubbing executing", "Log scrubbing complete"))
            {
                Log.CleanLogs(GetLogScrubbingMaximumAge(_settings));
            }

            return true; // repeat
        }

        public override Task<bool> PerformRunAsync(CancellationToken token)
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