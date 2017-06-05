using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarkdownSharp;
using Umbraco.Core.Logging;

namespace Umbraco.Web.HealthCheck
{
    internal class HealthCheckResults
    {
        private readonly Dictionary<string, IEnumerable<HealthCheckStatus>> _results;
        internal readonly bool AllChecksSuccessful;

        internal HealthCheckResults(IEnumerable<HealthCheck> checks)
        {
            _results = checks.ToDictionary(t => t.Name, t => t.GetStatus());

            // find out if all checks pass or not
            AllChecksSuccessful = true;
            foreach (var result in _results)
            {
                var checkIsSuccess = result.Value.All(x => x.ResultType == StatusResultType.Success);
                if (checkIsSuccess == false)
                {
                    AllChecksSuccessful = false;
                    break;
                }
            }
        }

        internal void LogResults()
        {
            LogHelper.Info<HealthCheckResults>("Scheduled health check results:");
            foreach (var result in _results)
            {
                var checkName = result.Key;
                var checkResults = result.Value;
                var checkIsSuccess = result.Value.All(x => x.ResultType == StatusResultType.Success);
                if (checkIsSuccess)
                {
                    LogHelper.Info<HealthCheckResults>(string.Format("    Checks for '{0}' all completed succesfully.", checkName));
                }
                else
                {
                    LogHelper.Warn<HealthCheckResults>(string.Format("    Checks for '{0}' completed with errors.", checkName));
                }

                foreach (var checkResult in checkResults)
                {
                    LogHelper.Info<HealthCheckResults>(string.Format("        Result: {0}, Message: '{1}'", checkResult.ResultType, checkResult.Message));
                }
            }
        }

        internal string ResultsAsMarkDown(bool slackMarkDown = false)
        {
            var newItem = "- ";
            if (slackMarkDown)
            {
                newItem = "• ";
            }

            var sb = new StringBuilder();

            foreach (var result in _results)
            {
                var checkName = result.Key;
                var checkResults = result.Value;
                var checkIsSuccess = result.Value.All(x => x.ResultType == StatusResultType.Success);
                if (checkIsSuccess)
                {
                    sb.AppendFormat("{0}Checks for'{1}' all completed succesfully.{2}", newItem, checkName, Environment.NewLine);
                }
                else
                {
                    sb.AppendFormat("{0}Checks for'{1}' completed with errors.{2}", newItem, checkName, Environment.NewLine);
                }

                foreach (var checkResult in checkResults)
                {
                    sb.AppendFormat("\t{0}Result:'{1}' , Message: '{2}'{3}", newItem, checkResult.ResultType, SimpleHtmlToMarkDown(checkResult.Message, slackMarkDown), Environment.NewLine);
                }
            }
            return sb.ToString();
        }

        internal string ResultsAsHtml()
        {
            Markdown mark = new Markdown();
            return mark.Transform(ResultsAsMarkDown());
        }
        private string SimpleHtmlToMarkDown(string html, bool slackMarkDown = false)
        {
            if (slackMarkDown)
            {
                return html.Replace("<strong>", "*")
                    .Replace("</strong>", "*")
                    .Replace("<em>", "_")
                    .Replace("</em>", "_");
            }
            return html.Replace("<strong>", "**")
                .Replace("</strong>", "**")
                .Replace("<em>", "*")
                .Replace("</em>", "*");
        }
    }
}
