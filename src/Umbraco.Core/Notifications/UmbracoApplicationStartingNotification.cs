namespace Umbraco.Cms.Core.Notifications
{
    /// <summary>
    /// Notification that occurs at the very end of the Umbraco boot process (after all <see cref="IComponent" />s are initialized).
    /// </summary>
    /// <seealso cref="Umbraco.Cms.Core.Notifications.INotification" />
    public class UmbracoApplicationStartingNotification : INotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoApplicationStartingNotification" /> class.
        /// </summary>
        /// <param name="runtimeLevel">The runtime level</param>
        public UmbracoApplicationStartingNotification(RuntimeLevel runtimeLevel) => RuntimeLevel = runtimeLevel;

        /// <summary>
        /// Gets the runtime level.
        /// </summary>
        /// <value>
        /// The runtime level.
        /// </value>
        public RuntimeLevel RuntimeLevel { get; }
    }
}
