using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Web.HealthCheck.NotificationMethods
{
    public interface IHealthCheckNotificationMethod
    {
        Task SendAsync(HealthCheckResults results, CancellationToken token);
    }
}
