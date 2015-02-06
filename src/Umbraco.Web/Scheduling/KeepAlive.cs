using System;
using System.Net;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;

namespace Umbraco.Web.Scheduling
{
    internal class KeepAlive
    {
        public static void Start(ApplicationContext appContext, IUmbracoSettingsSection settings)
        {
            using (DisposableTimer.DebugDuration<KeepAlive>(() => "Keep alive executing", () => "Keep alive complete"))
            {                
                var umbracoBaseUrl = ServerEnvironmentHelper.GetCurrentServerUmbracoBaseUrl(
                    appContext,
                    settings);

                if (string.IsNullOrWhiteSpace(umbracoBaseUrl))
                {
                    LogHelper.Warn<KeepAlive>("No url for service (yet), skip.");
                }
                else
                {
                    var url = string.Format("{0}ping.aspx", umbracoBaseUrl.EnsureEndsWith('/'));

                    try
                    {
                        using (var wc = new WebClient())
                        {
                            wc.DownloadString(url);
                        }
                    }
                    catch (Exception ee)
                    {
                        LogHelper.Error<KeepAlive>("Error in ping", ee);
                    }
                }
            }
            
        }
    }
}