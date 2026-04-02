using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Umbraco.Cms.Infrastructure.Notifications
{
    /// <summary>
    /// Notification that is raised when a recurring background job is triggered or executed.
    /// </summary>
    public class RecurringBackgroundJobNotification : ObjectNotification<IRecurringBackgroundJob>
    {
        /// <summary>
        /// Gets the recurring background job associated with this notification.
        /// </summary>
        public IRecurringBackgroundJob Job { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringBackgroundJobNotification"/> class.
        /// </summary>
        /// <param name="target">The recurring background job that triggered the notification.</param>
        /// <param name="messages">The <see cref="EventMessages"/> associated with this notification instance.</param>
        public RecurringBackgroundJobNotification(IRecurringBackgroundJob target, EventMessages messages) : base(target, messages) => Job = target;
    }
}
