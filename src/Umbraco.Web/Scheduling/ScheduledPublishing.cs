using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Publishing;
using Umbraco.Core.Sync;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Scheduling
{
    internal class ScheduledPublishing : DisposableObject, IBackgroundTask
    {
        private readonly ApplicationContext _appContext;
        private readonly IUmbracoSettingsSection _settings;

        private static bool _isPublishingRunning = false;

        public ScheduledPublishing(ApplicationContext appContext, IUmbracoSettingsSection settings)
        {
            _appContext = appContext;
            _settings = settings;
        }


        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
        }

        public void Run()
        {
            if (_appContext == null) return;

            using (DisposableTimer.DebugDuration<ScheduledPublishing>(() => "Scheduled publishing executing", () => "Scheduled publishing complete"))
            {
                if (_isPublishingRunning) return;

                _isPublishingRunning = true;

                try
                {
                    var umbracoBaseUrl = ServerEnvironmentHelper.GetCurrentServerUmbracoBaseUrl(_appContext, _settings);

                    if (string.IsNullOrWhiteSpace(umbracoBaseUrl))
                    {
                        LogHelper.Warn<ScheduledPublishing>("No url for service (yet), skip.");
                    }
                    else
                    {
                        var url = string.Format("{0}RestServices/ScheduledPublish/Index", umbracoBaseUrl.EnsureEndsWith('/'));
                        using (var wc = new WebClient())
                        {
                            //pass custom the authorization header
                            wc.Headers.Set("Authorization", AdminTokenAuthorizeAttribute.GetAuthHeaderTokenVal(_appContext));

                            var result = wc.UploadString(url, "");
                        }                        
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