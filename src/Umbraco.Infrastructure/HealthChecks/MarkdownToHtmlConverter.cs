using HeyRed.MarkdownSharp;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.HealthChecks.NotificationMethods;

namespace Umbraco.Cms.Infrastructure.HealthChecks;

public class MarkdownToHtmlConverter : IMarkdownToHtmlConverter
{
    public string ToHtml(HealthCheckResults results, HealthCheckNotificationVerbosity verbosity)
    {
        var mark = new Markdown();
        var html = mark.Transform(results.ResultsAsMarkDown(verbosity));
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
