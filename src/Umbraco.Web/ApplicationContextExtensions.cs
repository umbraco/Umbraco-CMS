using System;
using System.IO;
using System.Web;
using Umbraco.Core;

namespace Umbraco.Web
{
	public static class ApplicationContextExtensions
	{
		/// <summary>
		/// This will restart the application pool
		/// </summary>
		/// <param name="appContext"> </param>
		/// <param name="http"></param>
		public static void RestartApplicationPool(this ApplicationContext appContext, HttpContextBase http)
		{

			//we're going to put an application wide flag to show that the application is about to restart.
			//we're doing this because if there is a script checking if the app pool is fully restarted, then 
			//it can check if this flag exists...  if it does it means the app pool isn't restarted yet.
			http.Application.Add("AppPoolRestarting", true);

			//NOTE: this real way only works in full trust :(
			//HttpRuntime.UnloadAppDomain();
			//so we'll do the dodgy hack instead            
			var configPath = http.Request.PhysicalApplicationPath + "\\web.config";
			File.SetLastWriteTimeUtc(configPath, DateTime.UtcNow);
		}
	}
}
