using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Umbraco.Cms.Infrastructure.Notifications
{
    /// <summary>
    /// Notification that is raised when any recurring background job is about to start.
    /// This can be used to perform actions or logging before the job execution begins.
    /// </summary>
    public sealed class RecurringBackgroundJobStartingNotification : RecurringBackgroundJobNotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringBackgroundJobStartingNotification"/> class, which is raised when a recurring background job is starting.
        /// </summary>
        /// <param name="target">The <see cref="IRecurringBackgroundJob"/> instance representing the recurring background job that is starting.</param>
        /// <param name="messages">The <see cref="EventMessages"/> containing any event messages associated with the notification.</param>
        public RecurringBackgroundJobStartingNotification(IRecurringBackgroundJob target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
