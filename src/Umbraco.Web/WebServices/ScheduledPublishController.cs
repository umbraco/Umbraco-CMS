using System;
using System.Web.Mvc;
using umbraco;
using Umbraco.Core.Logging;
using Umbraco.Core.Publishing;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.WebServices
{
    /// <summary>
    /// A REST controller used for running the scheduled publishing, this is called from the background worker timer
    /// </summary>
    [AdminTokenAuthorize]
    public class ScheduledPublishController : UmbracoController
    {
        private static bool _isPublishingRunning = false;

        [HttpPost]
        public JsonResult Index()
        {
            if (_isPublishingRunning)
            {
                Logger.Debug<ScheduledPublishController>(() => "Scheduled publishing is currently executing this request will exit");
                return null;
            }
                
            _isPublishingRunning = true;

            try
            {
                // DO not run publishing if content is re-loading
                if (content.Instance.isInitializing == false)
                {
                    var publisher = new ScheduledPublisher(Services.ContentService);
                    var count = publisher.CheckPendingAndProcess();
                    Logger.Debug<ScheduledPublishController>(() => string.Format("The scheduler processed {0} items", count));
                }

                return Json(new
                {
                    success = true
                });

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
                LogHelper.Error<ScheduledPublishController>(errorMessage, ee);

                Response.StatusCode = 400; 

                return Json(new
                {
                    success = false,
                    message = ee.Message
                });
            }
            finally
            {
                _isPublishingRunning = false;
            }
        }
    }
}