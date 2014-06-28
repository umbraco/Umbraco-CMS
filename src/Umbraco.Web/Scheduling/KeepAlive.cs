using System;
using System.Net;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;

namespace Umbraco.Web.Scheduling
{
    internal class KeepAlive
    {
        public static void Start(object sender)
        {
            using (DisposableTimer.DebugDuration<KeepAlive>(() => "Keep alive executing", () => "Keep alive complete"))
            {                
                var umbracoBaseUrl = ServerEnvironmentHelper.GetCurrentServerUmbracoBaseUrl();

                var url = string.Format("{0}/ping.aspx", umbracoBaseUrl);

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