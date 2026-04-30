using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Umbraco.Cms.Infrastructure.Notifications
{
    /// <summary>
    /// Represents a notification that is raised when a recurring background job in Umbraco has stopped.
    /// </summary>
    public sealed class RecurringBackgroundJobStoppedNotification : RecurringBackgroundJobNotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringBackgroundJobStoppedNotification"/> class.
        /// </summary>
        /// <param name="target">The recurring background job that was stopped.</param>
        /// <param name="messages">The event messages associated with the stop event.</param>
        public RecurringBackgroundJobStoppedNotification(IRecurringBackgroundJob target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
