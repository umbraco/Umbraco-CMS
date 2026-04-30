using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Umbraco.Cms.Infrastructure.Notifications
{
    /// <summary>
    /// Notification that is raised when any recurring background job starts execution.
    /// This can be used to handle logic when a scheduled background process begins.
    /// </summary>
    public sealed class RecurringBackgroundJobStartedNotification : RecurringBackgroundJobNotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringBackgroundJobStartedNotification"/> class.
        /// </summary>
        /// <param name="target">The <see cref="IRecurringBackgroundJob"/> instance that has started.</param>
        /// <param name="messages">The <see cref="EventMessages"/> associated with the notification.</param>
        public RecurringBackgroundJobStartedNotification(IRecurringBackgroundJob target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
