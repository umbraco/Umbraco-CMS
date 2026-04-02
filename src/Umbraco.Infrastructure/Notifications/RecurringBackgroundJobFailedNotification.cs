using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Umbraco.Cms.Infrastructure.Notifications
{
    /// <summary>
    /// Represents a notification that is raised when a recurring background job fails to execute successfully.
    /// This can be used to handle or log failures of scheduled background tasks within the system.
    /// </summary>
    public sealed class RecurringBackgroundJobFailedNotification : RecurringBackgroundJobNotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringBackgroundJobFailedNotification"/> class, representing a notification that a recurring background job has failed.
        /// </summary>
        /// <param name="target">The <see cref="IRecurringBackgroundJob"/> instance that failed.</param>
        /// <param name="messages">The <see cref="EventMessages"/> containing details about the failure.</param>
        public RecurringBackgroundJobFailedNotification(IRecurringBackgroundJob target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
