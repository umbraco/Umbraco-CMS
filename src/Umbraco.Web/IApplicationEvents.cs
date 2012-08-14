using Umbraco.Core;
using umbraco.interfaces;

namespace Umbraco.Web
{
	/// <summary>
	/// Custom IApplicationStartupHandler that auto subscribes to the applications events
	/// </summary>
	public interface IApplicationEvents : IApplicationStartupHandler
	{
		void OnApplicationInitialized(UmbracoApplication httpApplication, ApplicationContext applicationContext);
		void OnApplicationStarting(UmbracoApplication httpApplication, ApplicationContext applicationContext);
		void OnApplicationStarted(UmbracoApplication httpApplication, ApplicationContext applicationContext);
	}
}