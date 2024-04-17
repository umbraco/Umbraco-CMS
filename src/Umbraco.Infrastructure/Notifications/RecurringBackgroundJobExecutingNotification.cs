using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Umbraco.Cms.Infrastructure.Notifications
{
    public sealed class RecurringBackgroundJobExecutingNotification : RecurringBackgroundJobNotification
    {
        public RecurringBackgroundJobExecutingNotification(IRecurringBackgroundJob target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
