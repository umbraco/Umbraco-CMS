using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Umbraco.Cms.Infrastructure.Notifications
{
    /// <summary>
    /// Notification that is triggered after the execution of a recurring background job.
    /// This can be used to perform actions or logging after a scheduled background task completes.
    /// </summary>
    public sealed class RecurringBackgroundJobExecutedNotification : RecurringBackgroundJobNotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringBackgroundJobExecutedNotification"/> class.
        /// </summary>
        /// <param name="target">The instance of the recurring background job that has just been executed.</param>
        /// <param name="messages">The <see cref="EventMessages"/> generated during the execution of the job.</param>
        public RecurringBackgroundJobExecutedNotification(IRecurringBackgroundJob target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
