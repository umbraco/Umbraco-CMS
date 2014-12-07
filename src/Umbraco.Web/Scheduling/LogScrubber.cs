using System;
using System.Web;
using System.Web.Caching;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Scheduling
{

    internal class LogScrubber : DisposableObject, IBackgroundTask
    {
        // this is a raw copy of the legacy code in all its uglyness
        private readonly ApplicationContext _appContext;

        public LogScrubber(ApplicationContext appContext)
        {
            _appContext = appContext;
        }

        private static int GetLogScrubbingMaximumAge()
        {
            int maximumAge = 24 * 60 * 60;
            try
            {
                if (global::umbraco.UmbracoSettings.MaxLogAge > -1)
                    maximumAge = global::umbraco.UmbracoSettings.MaxLogAge;
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
                Log.CleanLogs(GetLogScrubbingMaximumAge());
            }
        }
    }
}