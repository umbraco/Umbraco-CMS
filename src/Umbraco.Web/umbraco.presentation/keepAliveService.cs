using System;
using System.Diagnostics;
using System.Net;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace umbraco.presentation
{
	/// <summary>
	/// Makes a call to /umbraco/ping.aspx which is used to keep the web app alive
	/// </summary>
	public class keepAliveService
	{
        //NOTE: sender will be the umbraco ApplicationContext
		public static void PingUmbraco(object sender)
		{
			if (sender == null || !(sender is ApplicationContext))
				return;

		    var appContext = (ApplicationContext) sender;

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