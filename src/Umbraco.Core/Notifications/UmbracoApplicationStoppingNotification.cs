namespace Umbraco.Cms.Core.Notifications
{
    /// <summary>
    /// Notification that occurs when Umbraco is shutting down (after all <see cref="IComponent" />s are terminated).
    /// </summary>
    /// <seealso cref="Umbraco.Cms.Core.Notifications.INotification" />
    public class UmbracoApplicationStoppingNotification : INotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoApplicationStoppingNotification" /> class.
        /// </summary>
        public UmbracoApplicationStoppingNotification()
            : this(false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoApplicationStoppingNotification" /> class.
        /// </summary>
        /// <param name="isRestarting">Indicates whether Umbraco is restarting.</param>
        public UmbracoApplicationStoppingNotification(bool isRestarting) => IsRestarting = isRestarting;

        /// <summary>
        /// Gets a value indicating whether Umbraco is restarting (e.g. after an install or upgrade).
        /// </summary>
        /// <value>
        ///   <c>true</c> if Umbraco is restarting; otherwise, <c>false</c>.
        /// </value>
        public bool IsRestarting { get; }
    }
}
