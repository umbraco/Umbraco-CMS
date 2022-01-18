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
        public UmbracoApplicationStartingNotification(RuntimeLevel runtimeLevel)
            : this(runtimeLevel, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoApplicationStartingNotification" /> class.
        /// </summary>
        /// <param name="runtimeLevel">The runtime level</param>
        /// <param name="isRestarting">Indicates whether Umbraco is restarting.</param>
        public UmbracoApplicationStartingNotification(RuntimeLevel runtimeLevel, bool isRestarting)
        {
            RuntimeLevel = runtimeLevel;
            IsRestarting = isRestarting;
        }

        /// <summary>
        /// Gets the runtime level.
        /// </summary>
        /// <value>
        /// The runtime level.
        /// </value>
        public RuntimeLevel RuntimeLevel { get; }

        /// <summary>
        /// Gets a value indicating whether Umbraco is restarting (e.g. after an install or upgrade).
        /// </summary>
        /// <value>
        ///   <c>true</c> if Umbraco is restarting; otherwise, <c>false</c>.
        /// </value>
        public bool IsRestarting { get; }
    }
}
