﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HeyRed.MarkdownSharp;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.HealthCheck;

namespace Umbraco.Infrastructure.HealthCheck
{
    public class HealthCheckResults
    {
        private readonly Dictionary<string, IEnumerable<HealthCheckStatus>> _results;
        public readonly bool AllChecksSuccessful;

        private ILogger Logger => StaticApplicationLogging.Logger; // TODO: inject

        public HealthCheckResults(IEnumerable<Core.HealthCheck.HealthCheck> checks)
        {
            _results = checks.ToDictionary(
                t => t.Name,
                t => {
                    try
                    {
                        return t.GetStatus();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Error running scheduled health check: {HealthCheckName}", t.Name);
                        var message = $"Health check failed with exception: {ex.Message}. See logs for details.";
                        return new List<HealthCheckStatus>
                        {
                            new HealthCheckStatus(message)
                            {
                                ResultType = StatusResultType.Error
                            }
                        };
                    }
                });

            // find out if all checks pass or not
            AllChecksSuccessful = true;
            foreach (var result in _results)
            {
                var checkIsSuccess = result.Value.All(x => x.ResultType == StatusResultType.Success || x.ResultType == StatusResultType.Info || x.ResultType == StatusResultType.Warning);
                if (checkIsSuccess == false)
                {
                    AllChecksSuccessful = false;
                    break;
                }
            }
        }

        public void LogResults()
        {
            Logger.LogInformation("Scheduled health check results:");
            foreach (var result in _results)
            {
                var checkName = result.Key;
                var checkResults = result.Value;
                var checkIsSuccess = result.Value.All(x => x.ResultType == StatusResultType.Success);
                if (checkIsSuccess)
                {
                    Logger.LogInformation("Checks for '{HealthCheckName}' all completed successfully.", checkName);
                }
                else
                {
                    Logger.LogWarning("Checks for '{HealthCheckName}' completed with errors.", checkName);
                }

                foreach (var checkResult in checkResults)
                {
                    Logger.LogInformation("Result for {HealthCheckName}: {HealthCheckResult}, Message: '{HealthCheckMessage}'", checkName, checkResult.ResultType, checkResult.Message);
                }
            }
        }

        public string ResultsAsMarkDown(HealthCheckNotificationVerbosity verbosity)
        {
            var newItem = "- ";

            var sb = new StringBuilder();

            foreach (var result in _results)
            {
                var checkName = result.Key;
                var checkResults = result.Value;
                var checkIsSuccess = result.Value.All(x => x.ResultType == StatusResultType.Success);

                // add a new line if not the first check
                if (result.Equals(_results.First()) == false)
                {
                    sb.Append(Environment.NewLine);
                }

                if (checkIsSuccess)
                {
                    sb.AppendFormat("{0}Checks for '{1}' all completed successfully.{2}", newItem, checkName, Environment.NewLine);
                }
                else
                {
                    sb.AppendFormat("{0}Checks for '{1}' completed with errors.{2}", newItem, checkName, Environment.NewLine);
                }

                foreach (var checkResult in checkResults)
                {
                    sb.AppendFormat("\t{0}Result: '{1}'", newItem, checkResult.ResultType);

                    // With summary logging, only record details of warnings or errors
                    if (checkResult.ResultType != StatusResultType.Success || verbosity == HealthCheckNotificationVerbosity.Detailed)
                    {
                        sb.AppendFormat(", Message: '{0}'", SimpleHtmlToMarkDown(checkResult.Message));
                    }

                    sb.AppendLine(Environment.NewLine);
                }
            }

            return sb.ToString();
        }

        public string ResultsAsHtml(HealthCheckNotificationVerbosity verbosity)
        {
            var mark = new Markdown();
            var html = mark.Transform(ResultsAsMarkDown(verbosity));
            html = ApplyHtmlHighlighting(html);
            return html;
        }

        internal Dictionary<string, IEnumerable<HealthCheckStatus>> ResultsAsDictionary => _results;

        private string ApplyHtmlHighlighting(string html)
        {
            const string SuccessHexColor = "5cb85c";
            const string WarningHexColor = "f0ad4e";
            const string ErrorHexColor = "d9534f";

            html = ApplyHtmlHighlightingForStatus(html, StatusResultType.Success, SuccessHexColor);
            html = ApplyHtmlHighlightingForStatus(html, StatusResultType.Warning, WarningHexColor);
            return ApplyHtmlHighlightingForStatus(html, StatusResultType.Error, ErrorHexColor);
        }

        private string ApplyHtmlHighlightingForStatus(string html, StatusResultType status, string color)
        {
            return html
                .Replace("Result: '" + status + "'", "Result: <span style=\"color: #" + color + "\">" + status + "</span>");
        }

        private string SimpleHtmlToMarkDown(string html)
        {
            return html.Replace("<strong>", "**")
                .Replace("</strong>", "**")
                .Replace("<em>", "*")
                .Replace("</em>", "*");
        }

        public Dictionary<string, IEnumerable<HealthCheckStatus>> GetResultsForStatus(StatusResultType resultType)
        {
            switch (resultType)
            {
                case StatusResultType.Success:
                    // a check is considered a success status if all checks are successful or info
                    var successResults = _results.Where(x => x.Value.Any(y => y.ResultType == StatusResultType.Success) && x.Value.All(y => y.ResultType == StatusResultType.Success || y.ResultType == StatusResultType.Info));
                    return successResults.ToDictionary(x => x.Key, x => x.Value);
                case StatusResultType.Warning:
                    // a check is considered warn status if one check is warn and all others are success or info
                    var warnResults = _results.Where(x => x.Value.Any(y => y.ResultType == StatusResultType.Warning) && x.Value.All(y => y.ResultType == StatusResultType.Warning || y.ResultType == StatusResultType.Success || y.ResultType == StatusResultType.Info));
                    return warnResults.ToDictionary(x => x.Key, x => x.Value);
                case StatusResultType.Error:
                    // a check is considered error status if any check is error
                    var errorResults = _results.Where(x => x.Value.Any(y => y.ResultType == StatusResultType.Error));
                    return errorResults.ToDictionary(x => x.Key, x => x.Value);
                case StatusResultType.Info:
                    // a check is considered info status if all checks are info
                    var infoResults = _results.Where(x => x.Value.All(y => y.ResultType == StatusResultType.Info));
                    return infoResults.ToDictionary(x => x.Key, x => x.Value);
            }

            return null;
        }
    }
}
