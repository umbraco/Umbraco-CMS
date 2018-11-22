using System.Threading.Tasks;

namespace Umbraco.Web.HealthCheck.NotificationMethods
{
    public interface IHealthCheckNotificatationMethod
    {
        bool Enabled { get; }
        Task SendAsync(HealthCheckResults results);
    }
}
