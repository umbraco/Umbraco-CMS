using System;

namespace Umbraco.Core
{
	/// <summary>
	///  A bootstrapper interface for the Umbraco application
	/// </summary>
	public interface IBootManager
	{
		/// <summary>
		/// Fires first in the application startup process before any customizations can occur
		/// </summary>
		/// <returns></returns>
		IBootManager Initialize();

		/// <summary>
		/// Fires after initialization and calls the callback to allow for customizations to occur
		/// </summary>
		/// <param name="afterStartup"></param>
		/// <returns></returns>
		IBootManager Startup(Action<ApplicationContext> afterStartup);

		/// <summary>
		/// Fires after startup and calls the callback once customizations are locked
		/// </summary>
		/// <param name="afterComplete"></param>
		/// <returns></returns>
		IBootManager Complete(Action<ApplicationContext> afterComplete);

	}
}