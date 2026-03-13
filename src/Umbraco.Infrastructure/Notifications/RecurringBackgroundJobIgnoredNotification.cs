using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Umbraco.Cms.Infrastructure.Notifications
{
    /// <summary>
    /// Notification raised when a recurring background job is ignored, typically because it is already running or has been disabled.
    /// </summary>
    public sealed class RecurringBackgroundJobIgnoredNotification : RecurringBackgroundJobNotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringBackgroundJobIgnoredNotification"/> class.
        /// This notification is triggered when a recurring background job is ignored and not executed.
        /// </summary>
        /// <param name="target">The instance of the recurring background job that was ignored.</param>
        /// <param name="messages">The <see cref="EventMessages"/> containing any messages related to the ignored job.</param>
        public RecurringBackgroundJobIgnoredNotification(IRecurringBackgroundJob target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
