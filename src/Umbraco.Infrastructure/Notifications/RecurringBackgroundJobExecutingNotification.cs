using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Umbraco.Cms.Infrastructure.Notifications
{
    /// <summary>
    /// Notification that is raised immediately before a recurring background job is executed.
    /// This can be used to perform actions or checks prior to the job's execution.
    /// </summary>
    public sealed class RecurringBackgroundJobExecutingNotification : RecurringBackgroundJobNotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringBackgroundJobExecutingNotification"/> class.
        /// </summary>
        /// <param name="target">The <see cref="IRecurringBackgroundJob"/> that is currently executing.</param>
        /// <param name="messages">The <see cref="EventMessages"/> associated with the job execution.</param>
        public RecurringBackgroundJobExecutingNotification(IRecurringBackgroundJob target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
