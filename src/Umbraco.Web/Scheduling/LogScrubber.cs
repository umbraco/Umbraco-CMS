using System;
using System.Web;
using System.Web.Caching;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Scheduling
{

    internal class LogScrubber : DisposableObject, IBackgroundTask
    {
        private readonly ApplicationContext _appContext;
        private readonly IUmbracoSettingsSection _settings;

        public LogScrubber(ApplicationContext appContext, IUmbracoSettingsSection settings)
        {
            _appContext = appContext;
            _settings = settings;
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

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {         
        }

        public void Run()
        {
            using (DisposableTimer.DebugDuration<LogScrubber>(() => "Log scrubbing executing", () => "Log scrubbing complete"))
            {
                Log.CleanLogs(GetLogScrubbingMaximumAge(_settings));
            }
        }
    }
}