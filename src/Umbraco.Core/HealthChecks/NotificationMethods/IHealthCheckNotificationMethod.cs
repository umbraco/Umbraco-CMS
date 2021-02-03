using System.Threading.Tasks;
using Umbraco.Core.Composing;

namespace Umbraco.Core.HealthChecks.NotificationMethods
{
    public interface IHealthCheckNotificationMethod : IDiscoverable
    {
        bool Enabled { get; }

        Task SendAsync(HealthCheckResults results);
    }
}
