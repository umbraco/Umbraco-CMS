using System;
using System.Diagnostics;
using System.Net;
using System.Web;

namespace umbraco.presentation
{
	/// <summary>
	/// Summary description for keepAliveService.
	/// </summary>
	public class keepAliveService
	{
		public static void PingUmbraco(object sender)
		{
			if (sender == null)
				return;
			string url = string.Format("http://{0}/ping.aspx", ((HttpContext)sender).Application["umbracoUrl"]);
			try
			{
				using (WebClient wc = new WebClient())
				{
					wc.DownloadString(url);
				}
			}
			catch(Exception ee)
			{
				Debug.Write(string.Format("Error in ping({0}) -> {1}", url, ee));
			}
		}
	}
}