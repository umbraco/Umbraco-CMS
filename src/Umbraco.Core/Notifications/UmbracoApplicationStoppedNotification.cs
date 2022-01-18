namespace Umbraco.Cms.Core.Notifications
{
    /// <summary>
    /// Notification that occurs when Umbraco has completely shutdown.
    /// </summary>
    /// <seealso cref="Umbraco.Cms.Core.Notifications.INotification" />
    public class UmbracoApplicationStoppedNotification : INotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoApplicationStoppedNotification" /> class.
        /// </summary>
        public UmbracoApplicationStoppedNotification()
            : this(false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoApplicationStoppedNotification" /> class.
        /// </summary>
        /// <param name="isRestarting">Indicates whether Umbraco is restarting.</param>
        public UmbracoApplicationStoppedNotification(bool isRestarting) => IsRestarting = isRestarting;

        /// <summary>
        /// Gets a value indicating whether Umbraco is restarting (e.g. after an install or upgrade).
        /// </summary>
        /// <value>
        ///   <c>true</c> if Umbraco is restarting; otherwise, <c>false</c>.
        /// </value>
        public bool IsRestarting { get; }
    }
}
