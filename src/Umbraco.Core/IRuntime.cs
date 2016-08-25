using System;
using LightInject;

namespace Umbraco.Core
{
    // fixme - move!
    public class RuntimeState
    {
        /// <summary>
        /// Gets a value indicating whether the application is running in debug mode.
        /// </summary>
        public bool Debug { get; }

        public RuntimeSomething Something { get; }
    }

    public enum RuntimeSomething
    {
        Boot,
        Run
    }

	/// <summary>
	/// Defines the Umbraco runtime.
	/// </summary>
	public interface IRuntime
	{
        /// <summary>
        /// Boots the runtime.
        /// </summary>
        /// <param name="container">The application service container.</param>
	    void Boot(ServiceContainer container);

        /// <summary>
        /// Terminates the runtime.
        /// </summary>
	    void Terminate();

        // fixme - everything below is obsolete!

		/// <summary>
		/// Fires first in the application startup process before any customizations can occur
		/// </summary>
		/// <returns></returns>
		IRuntime Initialize();

		/// <summary>
		/// Fires after initialization and calls the callback to allow for customizations to occur
		/// </summary>
		/// <param name="afterStartup"></param>
		/// <returns></returns>
		IRuntime Startup(Action<ApplicationContext> afterStartup);

		/// <summary>
		/// Fires after startup and calls the callback once customizations are locked
		/// </summary>
		/// <param name="afterComplete"></param>
		/// <returns></returns>
		IRuntime Complete(Action<ApplicationContext> afterComplete);

	}
}