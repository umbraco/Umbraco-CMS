namespace Umbraco.Cms.Core.HealthChecks.NotificationMethods;

/// <summary>
///     Defines a converter for transforming health check results from markdown to HTML.
/// </summary>
public interface IMarkdownToHtmlConverter
{
    /// <summary>
    ///     Converts health check results to HTML format.
    /// </summary>
    /// <param name="results">The health check results to convert.</param>
    /// <param name="verbosity">The verbosity level for the output.</param>
    /// <returns>An HTML-formatted string representation of the health check results.</returns>
    string ToHtml(HealthCheckResults results, HealthCheckNotificationVerbosity verbosity);
}
