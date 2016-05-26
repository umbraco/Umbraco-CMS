using System;
using System.Web.Mvc;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.WebServices
{
    /// <summary>
    /// A REST controller used for running the scheduled publishing, this is called from the background worker timer
    /// </summary>
    [AdminTokenAuthorize]
    public class ScheduledPublishController : UmbracoController
    {
        private static bool _isPublishingRunning;
        private static readonly object Locker = new object();

        [HttpPost]
        public JsonResult Index()
        {
            lock (Locker)
            {
                if (_isPublishingRunning)
                    return null;
                _isPublishingRunning = true;
            }

            try
            {
                // ensure we have everything we need
                if (ApplicationContext.IsReady == false) return null;
                Services.ContentService.WithResult().PerformScheduledPublish();
                return Json(new { success = true });
            }
            catch (Exception ee)
            {
                LogHelper.Error<ScheduledPublishController>("Error executing scheduled task", ee);

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