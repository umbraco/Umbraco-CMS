namespace Umbraco.Core.HealthChecks.NotificationMethods
{
    public interface IMarkdownToHtmlConverter
    {
        string ToHtml(HealthCheckResults results, HealthCheckNotificationVerbosity verbosity);
    }
}
