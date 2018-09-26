using System;
using System.Net.Http;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace umbraco.presentation
{
	[Obsolete("This is no longer used and will be removed in future versions")]
	public class keepAliveService
	{
	    private static HttpClient _httpClient;
        //NOTE: sender will be the umbraco ApplicationContext
        public static void PingUmbraco(object sender)
		{
			if (sender == null || !(sender is ApplicationContext))
				return;

		    var appContext = (ApplicationContext) sender;

		    var url = appContext.UmbracoApplicationUrl + "/ping.aspx";
			try
			{
			    if (_httpClient == null)
			        _httpClient = new HttpClient();

			    using (var request = new HttpRequestMessage(HttpMethod.Get, url))
			    {
			        var response = _httpClient.SendAsync(request).Result;
			    }
			}
			catch(Exception ee)
			{
                LogHelper.Debug<keepAliveService>(string.Format("Error in ping({0}) -> {1}", url, ee));
			}
		}
	}
}
