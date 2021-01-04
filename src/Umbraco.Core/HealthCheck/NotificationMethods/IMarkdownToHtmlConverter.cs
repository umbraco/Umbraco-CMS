using Umbraco.Core.HealthCheck;
using Umbraco.Infrastructure.HealthCheck;

namespace Umbraco.Web.HealthCheck.NotificationMethods
{
    public interface IMarkdownToHtmlConverter
    {
        string ToHtml(HealthCheckResults results, HealthCheckNotificationVerbosity verbosity);
    }
}
