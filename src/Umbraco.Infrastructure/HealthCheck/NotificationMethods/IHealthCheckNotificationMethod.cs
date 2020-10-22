using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core.Composing;
using Umbraco.Infrastructure.HealthCheck;

namespace Umbraco.Web.HealthCheck.NotificationMethods
{
    public interface IHealthCheckNotificationMethod : IDiscoverable
    {
        bool Enabled { get; }
        Task SendAsync(HealthCheckResults results, CancellationToken token);
    }
}
