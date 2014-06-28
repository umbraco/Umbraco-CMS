using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Publishing;
using Umbraco.Core.Sync;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Scheduling
{
    internal class ScheduledPublishing
    {
        private static bool _isPublishingRunning = false;

        public void Start(ApplicationContext appContext)
        {
            if (appContext == null) return;

            using (DisposableTimer.DebugDuration<ScheduledPublishing>(() => "Scheduled publishing executing", () => "Scheduled publishing complete"))
            {                                
                if (_isPublishingRunning) return;

                _isPublishingRunning = true;
            
                try
                {
                    var umbracoBaseUrl = ServerEnvironmentHelper.GetCurrentServerUmbracoBaseUrl();
                    var url = string.Format("{0}/RestServices/ScheduledPublish/Index", umbracoBaseUrl);
                    using (var wc = new WebClient())
                    {
                        //pass custom the authorization header
                        wc.Headers.Set("Authorization", AdminTokenAuthorizeAttribute.GetAuthHeaderTokenVal(appContext));

                        var result = wc.UploadString(url, "");
                    }
                }
                catch (Exception ee)
                {
                    LogHelper.Error<ScheduledPublishing>("An error occurred with the scheduled publishing", ee);
                }
                finally
                {
                    _isPublishingRunning = false;
                }
            }            
        }

        
    }
}