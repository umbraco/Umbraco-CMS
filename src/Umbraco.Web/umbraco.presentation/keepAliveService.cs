using System;
using System.Diagnostics;
using System.Net;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace umbraco.presentation
{
	[Obsolete("This is no longer used and will be removed in future versions")]
	public class keepAliveService
	{
        //NOTE: sender will be the umbraco ApplicationContext
		public static void PingUmbraco(object sender)
		{
			if (sender == null || !(sender is ApplicationContext))
				return;

		    var appContext = (ApplicationContext) sender;

            //TODO: This won't always work, in load balanced scenarios ping will not work because 
            // this original request url will be public and not internal to the server.
            var url = string.Format("http://{0}/ping.aspx", appContext.OriginalRequestUrl);
			try
			{
				using (var wc = new WebClient())
				{
					wc.DownloadString(url);
				}
			}
			catch(Exception ee)
			{
                LogHelper.Debug<keepAliveService>(string.Format("Error in ping({0}) -> {1}", url, ee));
			}
		}
	}
}