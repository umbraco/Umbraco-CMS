using umbraco.interfaces;

namespace Umbraco.Core
{
    /// <summary>
	/// Custom IApplicationStartupHandler that auto subscribes to the applications events
	/// </summary>
	public interface IApplicationEventHandler : IApplicationStartupHandler
	{
		/// <summary>
		/// ApplicationContext is created and other static objects that require initialization have been setup
		/// </summary>
		/// <param name="umbracoApplication"></param>
		/// <param name="applicationContext"></param>
		void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext);
		
		/// <summary>
		/// All resolvers have been initialized but resolution is not frozen so they can be modified in this method
		/// </summary>
        /// <param name="umbracoApplication"></param>
		/// <param name="applicationContext"></param>
        void OnApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext);
		
		/// <summary>
		/// Bootup is completed, this allows you to perform any other bootup logic required for the application.
		/// Resolution is frozen so now they can be used to resolve instances.
		/// </summary>
        /// <param name="umbracoApplication"></param>
		/// <param name="applicationContext"></param>
        void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext);
	}
}