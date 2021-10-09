using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace Umbraco.Cms.Infrastructure.Notifications
{
    public class RecurringHostedServiceNotification : ObjectNotification<RecurringHostedServiceBase>
    {
        public RecurringHostedServiceBase Service { get; }
        public RecurringHostedServiceNotification(RecurringHostedServiceBase target, EventMessages messages) : base(target, messages) => Service = target;
    }
}
