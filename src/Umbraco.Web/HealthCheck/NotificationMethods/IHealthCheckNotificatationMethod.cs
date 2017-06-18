using System.Threading.Tasks;

namespace Umbraco.Web.HealthCheck.NotificationMethods
{
    public interface IHealthCheckNotificatationMethod
    {
        Task SendAsync(HealthCheckResults results);
    }
}
