using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Umbraco.Cms.Infrastructure.Notifications
{
    public sealed class RecurringBackgroundJobStoppingNotification : RecurringBackgroundJobNotification
    {
        public RecurringBackgroundJobStoppingNotification(IRecurringBackgroundJob target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
