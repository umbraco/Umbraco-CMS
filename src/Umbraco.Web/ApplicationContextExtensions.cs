using System.Web;
using Umbraco.Core;

namespace Umbraco.Web
{
	public static class ApplicationContextExtensions
	{
	    /// <summary>
	    /// Restarts the application pool by unloading the application domain.
	    /// </summary>
	    /// <param name="appContext"></param>
	    /// <param name="http"></param>
	    public static void RestartApplicationPool(this ApplicationContext appContext, HttpContextBase http)
	    {
            // we're going to put an application wide flag to show that the application is about to restart.
            // we're doing this because if there is a script checking if the app pool is fully restarted, then
            // it can check if this flag exists...  if it does it means the app pool isn't restarted yet.
            http.Application.Add("AppPoolRestarting", true);

            // unload app domain
			HttpRuntime.UnloadAppDomain();
		}
	}
}
