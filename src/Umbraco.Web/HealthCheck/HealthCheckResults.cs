using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Logging;

namespace Umbraco.Web.HealthCheck
{
    public class HealthCheckResults
    {
        private readonly Dictionary<string, IEnumerable<HealthCheckStatus>> _results;

        public HealthCheckResults(IEnumerable<HealthCheck> checks)
        {
            _results = checks.ToDictionary(t => t.Name, t => t.GetStatus());
        }

        public void LogResults()
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

        public string ResultsAsMarkDown()
        {
            var newItem = "- ";
            var sb = new StringBuilder();

            foreach (var result in _results)
            {
                var checkName = result.Key;
                var checkResults = result.Value;
                var checkIsSuccess = result.Value.All(x => x.ResultType == StatusResultType.Success);
                if (checkIsSuccess)
                {
                    sb.AppendFormat("{0}Checks for'{1}' all completed succesfully.", newItem, checkName);
                }
                else
                {
                    sb.AppendFormat("{0}Checks for'{1}' completed with errors.", newItem, checkName);
                }

                foreach (var checkResult in checkResults)
                {
                    sb.AppendFormat("{0}{1}Result:'{2}' , Message: '{3}'", newItem, newItem, checkResult.ResultType, checkResult.Message);
                }
            }
            return sb.ToString();
        }
    }
}
