using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace Umbraco.Cms.Infrastructure.Notifications
{
    public sealed class RecurringHostedServiceFailedNotification : RecurringHostedServiceNotification
    {
        public RecurringHostedServiceFailedNotification(RecurringHostedServiceBase target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
