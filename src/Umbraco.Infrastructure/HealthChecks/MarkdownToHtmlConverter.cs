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
        const string SuccessHexColor = "5cb85c";
        const string WarningHexColor = "f0ad4e";
        const string ErrorHexColor = "d9534f";

        html = ApplyHtmlHighlightingForStatus(html, StatusResultType.Success, SuccessHexColor);
        html = ApplyHtmlHighlightingForStatus(html, StatusResultType.Warning, WarningHexColor);
        return ApplyHtmlHighlightingForStatus(html, StatusResultType.Error, ErrorHexColor);
    }

    private string ApplyHtmlHighlightingForStatus(string html, StatusResultType status, string color) =>
        html
            .Replace("Result: '" + status + "'", "Result: <span style=\"color: #" + color + "\">" + status + "</span>");
}
