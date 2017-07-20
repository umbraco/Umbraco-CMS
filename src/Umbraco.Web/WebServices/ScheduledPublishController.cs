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
                var errorMessage = "Error executing scheduled task";
                if (HttpContext != null && HttpContext.Request != null)
                {
                    if (HttpContext.Request.Url != null)
                        errorMessage = string.Format("{0} | Request to {1}", errorMessage, HttpContext.Request.Url);
                    if (HttpContext.Request.UserHostAddress != null)
                        errorMessage = string.Format("{0} | Coming from {1}", errorMessage, HttpContext.Request.UserHostAddress);
                    if (HttpContext.Request.UrlReferrer != null)
                        errorMessage = string.Format("{0} | Referrer {1}", errorMessage, HttpContext.Request.UrlReferrer);
                }
                Logger.Error<ScheduledPublishController>(errorMessage, ee);

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
