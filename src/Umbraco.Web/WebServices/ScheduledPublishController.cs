using System;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.WebServices
{
    /// <summary>
    /// Represents a REST controller used for running the scheduled publishing.
    /// </summary>
    [AdminTokenAuthorize]
    public class ScheduledPublishController : UmbracoController
    {
        private static bool _isPublishingRunning;
        private static readonly object Locker = new object();
        private readonly IRuntimeState _runtime;

        public ScheduledPublishController(IRuntimeState runtime)
        {
            _runtime = runtime;
        }

        [HttpPost]
        public JsonResult Index()
        {
            lock (Locker)
            {
                if (_isPublishingRunning)
                {
                    Logger.Debug<ScheduledPublishController>(() => "Already executing, skipping.");
                    return null;
                }

                _isPublishingRunning = true;
            }

            try
            {
                // ensure we have everything we need
                if (_runtime.Level != RuntimeLevel.Run) return null;
                Services.ContentService.WithResult().PerformScheduledPublish();
                return Json(new { success = true });
            }
            catch (Exception ee)
            {
                Logger.Error<ScheduledPublishController>("Error executing scheduled task", ee);

                Response.StatusCode = 400; 

                return Json(new
                {
                    success = false,
                    message = ee.Message
                });
            }
            finally
            {
                lock (Locker)
                {
                    _isPublishingRunning = false;
                }
            }
        }
    }
}