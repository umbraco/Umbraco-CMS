using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.HealthChecks.NotificationMethods;

namespace Umbraco.Cms.Infrastructure.HealthChecks;

/// <summary>
/// Provides functionality to convert Markdown-formatted text into HTML.
/// </summary>
public class MarkdownToHtmlConverter : IMarkdownToHtmlConverter
{
    private readonly Core.Strings.IMarkdownToHtmlConverter _markdownToHtmlConverter;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.HealthChecks.MarkdownToHtmlConverter"/> class,
    /// using the specified markdown to HTML converter implementation.
    /// </summary>
    /// <param name="markdownToHtmlConverter">An implementation of <see cref="IMarkdownToHtmlConverter"/> used to convert markdown content to HTML.</param>
    public MarkdownToHtmlConverter(Core.Strings.IMarkdownToHtmlConverter markdownToHtmlConverter) => _markdownToHtmlConverter = markdownToHtmlConverter;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownToHtmlConverter"/> class.
    /// This is the parameterless constructor.
    /// </summary>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public MarkdownToHtmlConverter()
        : this(StaticServiceProvider.Instance.GetRequiredService<Core.Strings.IMarkdownToHtmlConverter>())
    {
    }

    /// <summary>
    /// Converts the specified health check results to an HTML string by first generating a Markdown representation
    /// of the results (according to the given verbosity), converting that Markdown to HTML, and then applying additional HTML highlighting.
    /// </summary>
    /// <param name="results">The <see cref="HealthCheckResults"/> to convert to HTML.</param>
    /// <param name="verbosity">The verbosity level that determines the detail included in the Markdown representation.</param>
    /// <returns>An HTML string representing the health check results, with highlighting applied.</returns>
    public string ToHtml(HealthCheckResults results, HealthCheckNotificationVerbosity verbosity)
    {
        var html = _markdownToHtmlConverter.ToHtml(results.ResultsAsMarkDown(verbosity));
        html = ApplyHtmlHighlighting(html);
        return html;
    }

    private string ApplyHtmlHighlighting(string html)
    {
        const string successHexColor = "5cb85c";
        const string warningHexColor = "f0ad4e";
        const string errorHexColor = "d9534f";

        html = ApplyHtmlHighlightingForStatus(html, StatusResultType.Success, successHexColor);
        html = ApplyHtmlHighlightingForStatus(html, StatusResultType.Warning, warningHexColor);
        return ApplyHtmlHighlightingForStatus(html, StatusResultType.Error, errorHexColor);
    }

    private string ApplyHtmlHighlightingForStatus(string html, StatusResultType status, string color) =>
        html
            .Replace("Result: '" + status + "'", "Result: <span style=\"color: #" + color + "\">" + status + "</span>");
}
