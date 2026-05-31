using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Umbraco.Cms.Infrastructure.Notifications
{
    /// <summary>
    /// Notification that is raised by the application when a recurring background job is stopping.
    /// This can be used to perform cleanup or respond to the job's shutdown event.
    /// </summary>
    public sealed class RecurringBackgroundJobStoppingNotification : RecurringBackgroundJobNotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringBackgroundJobStoppingNotification"/> class.
        /// </summary>
        /// <param name="target">The instance of <see cref="IRecurringBackgroundJob"/> representing the recurring background job that is stopping.</param>
        /// <param name="messages">The <see cref="EventMessages"/> containing any messages associated with the stopping notification.</param>
        public RecurringBackgroundJobStoppingNotification(IRecurringBackgroundJob target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
