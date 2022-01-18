namespace Umbraco.Cms.Core.Notifications
{
    /// <summary>
    /// Notification that occurs when Umbraco has completely booted up and the request processing pipeline is configured.
    /// </summary>
    /// <seealso cref="Umbraco.Cms.Core.Notifications.INotification" />
    public class UmbracoApplicationStartedNotification : INotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoApplicationStartedNotification" /> class.
        /// </summary>
        public UmbracoApplicationStartedNotification()
            : this(false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoApplicationStartedNotification" /> class.
        /// </summary>
        /// <param name="isRestarting">Indicates whether Umbraco is restarting.</param>
        public UmbracoApplicationStartedNotification(bool isRestarting) => IsRestarting = isRestarting;

        /// <summary>
        /// Gets a value indicating whether Umbraco is restarting (e.g. after an install or upgrade).
        /// </summary>
        /// <value>
        ///   <c>true</c> if Umbraco is restarting; otherwise, <c>false</c>.
        /// </value>
        public bool IsRestarting { get; }
    }
}
