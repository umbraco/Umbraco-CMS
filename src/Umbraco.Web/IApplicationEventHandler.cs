using Umbraco.Core;
using umbraco.interfaces;

namespace Umbraco.Web
{
	/// <summary>
	/// Custom IApplicationStartupHandler that auto subscribes to the applications events
	/// </summary>
	public interface IApplicationEventHandler : IApplicationStartupHandler
	{
		/// <summary>
		/// ApplicationContext is created and other static objects that require initialization have been setup
		/// </summary>
		/// <param name="httpApplication"></param>
		/// <param name="applicationContext"></param>
		void OnApplicationInitialized(UmbracoApplication httpApplication, ApplicationContext applicationContext);
		
		/// <summary>
		/// All resolvers have been initialized but resolution is not frozen so they can be modified in this method
		/// </summary>
		/// <param name="httpApplication"></param>
		/// <param name="applicationContext"></param>
		void OnApplicationStarting(UmbracoApplication httpApplication, ApplicationContext applicationContext);
		
		/// <summary>
		/// Bootup is completed, this allows you to perform any other bootup logic required for the application.
		/// Resolution is frozen so now they can be used to resolve instances.
		/// </summary>
		/// <param name="httpApplication"></param>
		/// <param name="applicationContext"></param>
		void OnApplicationStarted(UmbracoApplication httpApplication, ApplicationContext applicationContext);
	}
}