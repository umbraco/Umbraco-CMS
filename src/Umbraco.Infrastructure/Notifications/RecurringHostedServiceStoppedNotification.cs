using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace Umbraco.Cms.Infrastructure.Notifications
{
    public sealed class RecurringHostedServiceStoppedNotification : RecurringHostedServiceNotification
    {
        public RecurringHostedServiceStoppedNotification(RecurringHostedServiceBase target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
