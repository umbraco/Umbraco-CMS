using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Web.HealthCheck.NotificationMethods
{
    public interface IHealthCheckNotificationMethod
    {
        bool Enabled { get; }
        Task SendAsync(HealthCheckResults results, CancellationToken token);
    }
}
