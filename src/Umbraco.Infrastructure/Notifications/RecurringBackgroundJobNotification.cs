using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Umbraco.Cms.Infrastructure.Notifications
{
    public class RecurringBackgroundJobNotification : ObjectNotification<IRecurringBackgroundJob>
    {
        public IRecurringBackgroundJob Job { get; }
        public RecurringBackgroundJobNotification(IRecurringBackgroundJob target, EventMessages messages) : base(target, messages) => Job = target;
    }
}
