using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace Umbraco.Cms.Infrastructure.Notifications
{
    public sealed class RecurringHostedServiceStoppingNotification : RecurringHostedServiceNotification
    {
        public RecurringHostedServiceStoppingNotification(RecurringHostedServiceBase target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
