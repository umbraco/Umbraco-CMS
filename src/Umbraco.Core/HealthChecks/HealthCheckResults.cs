using System.Text;
using Microsoft.Extensions.Logging;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.HealthChecks;

/// <summary>
///     Represents the results of executing a collection of health checks.
/// </summary>
public class HealthCheckResults
{
    // TODO (V18): Convert to property to comply with SA1401 (fields should be private)
    /// <summary>
    ///     Indicates whether all health checks completed successfully.
    /// </summary>
    public readonly bool AllChecksSuccessful;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HealthCheckResults" /> class.
    /// </summary>
    /// <param name="results">The dictionary of health check results keyed by check name.</param>
    /// <param name="allChecksSuccessful">A value indicating whether all checks were successful.</param>
    private HealthCheckResults(Dictionary<string, IEnumerable<HealthCheckStatus>> results, bool allChecksSuccessful)
    {
        ResultsAsDictionary = results;
        AllChecksSuccessful = allChecksSuccessful;
    }

    /// <summary>
    ///     Gets the health check results as a dictionary keyed by check name.
    /// </summary>
    internal Dictionary<string, IEnumerable<HealthCheckStatus>> ResultsAsDictionary { get; }

    private static ILogger Logger => StaticApplicationLogging.Logger; // TODO: inject

    /// <summary>
    ///     Creates a new <see cref="HealthCheckResults" /> instance by executing the specified health checks.
    /// </summary>
    /// <param name="checks">The health checks to execute.</param>
    /// <returns>A task that represents the asynchronous operation containing the health check results.</returns>
    public static async Task<HealthCheckResults> Create(IEnumerable<HealthCheck> checks)
    {
        Dictionary<string, IEnumerable<HealthCheckStatus>> results = await checks.ToDictionaryAsync(
            t => t.Name,
            async t =>
            {
                try
                {
                    return await t.GetStatusAsync();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error running scheduled health check: {HealthCheckName}", t.Name);
                    var message = $"Health check failed with exception: {ex.Message}. See logs for details.";
                    return new List<HealthCheckStatus> { new(message) { ResultType = StatusResultType.Error } };
                }
            });

        // find out if all checks pass or not
        var allChecksSuccessful = true;
        foreach (KeyValuePair<string, IEnumerable<HealthCheckStatus>> result in results)
        {
            var checkIsSuccess = result.Value.All(x =>
                x.ResultType == StatusResultType.Success || x.ResultType == StatusResultType.Info ||
                x.ResultType == StatusResultType.Warning);
            if (checkIsSuccess == false)
            {
                allChecksSuccessful = false;
                break;
            }
        }

        return new HealthCheckResults(results, allChecksSuccessful);
    }

    /// <summary>
    ///     Creates a new <see cref="HealthCheckResults" /> instance by executing a single health check.
    /// </summary>
    /// <param name="check">The health check to execute.</param>
    /// <returns>A task that represents the asynchronous operation containing the health check results.</returns>
    public static async Task<HealthCheckResults> Create(HealthCheck check) => await Create(new List<HealthCheck>() { check });

    /// <summary>
    ///     Logs the health check results to the application logger.
    /// </summary>
    public void LogResults()
    {
        Logger.LogInformation("Scheduled health check results:");
        foreach (KeyValuePair<string, IEnumerable<HealthCheckStatus>> result in ResultsAsDictionary)
        {
            var checkName = result.Key;
            IEnumerable<HealthCheckStatus> checkResults = result.Value;
            var checkIsSuccess = result.Value.All(x => x.ResultType == StatusResultType.Success);
            if (checkIsSuccess)
            {
                Logger.LogInformation("Checks for '{HealthCheckName}' all completed successfully.", checkName);
            }
            else
            {
                Logger.LogWarning("Checks for '{HealthCheckName}' completed with errors.", checkName);
            }

            foreach (HealthCheckStatus checkResult in checkResults)
            {
                Logger.LogInformation(
                    "Result for {HealthCheckName}: {HealthCheckResult}, Message: '{HealthCheckMessage}'",
                    checkName,
                    checkResult.ResultType,
                    checkResult.Message);
            }
        }
    }

    /// <summary>
    ///     Converts the health check results to a markdown-formatted string.
    /// </summary>
    /// <param name="verbosity">The verbosity level for the output.</param>
    /// <returns>A markdown-formatted string representation of the results.</returns>
    public string ResultsAsMarkDown(HealthCheckNotificationVerbosity verbosity)
    {
        var newItem = "- ";

        var sb = new StringBuilder();

        foreach (KeyValuePair<string, IEnumerable<HealthCheckStatus>> result in ResultsAsDictionary)
        {
            var checkName = result.Key;
            IEnumerable<HealthCheckStatus> checkResults = result.Value;
            var checkIsSuccess = result.Value.All(x => x.ResultType == StatusResultType.Success);

            // add a new line if not the first check
            if (result.Equals(ResultsAsDictionary.First()) == false)
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

            foreach (HealthCheckStatus checkResult in checkResults)
            {
                sb.AppendFormat("\t{0}Result: '{1}'", newItem, checkResult.ResultType);

                // With summary logging, only record details of warnings or errors
                if (checkResult.ResultType != StatusResultType.Success ||
                    verbosity == HealthCheckNotificationVerbosity.Detailed)
                {
                    sb.AppendFormat(", Message: '{0}'", SimpleHtmlToMarkDown(checkResult.Message));
                }

                sb.AppendLine(Environment.NewLine);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Gets the health check results filtered by the specified status result type.
    /// </summary>
    /// <param name="resultType">The status result type to filter by.</param>
    /// <returns>A dictionary of health check results matching the specified status, or null if the result type is not recognized.</returns>
    public Dictionary<string, IEnumerable<HealthCheckStatus>>? GetResultsForStatus(StatusResultType resultType)
    {
        switch (resultType)
        {
            case StatusResultType.Success:
                // a check is considered a success status if all checks are successful or info
                IEnumerable<KeyValuePair<string, IEnumerable<HealthCheckStatus>>> successResults =
                    ResultsAsDictionary.Where(x =>
                        x.Value.Any(y => y.ResultType == StatusResultType.Success) && x.Value.All(y =>
                            y.ResultType == StatusResultType.Success || y.ResultType == StatusResultType.Info));
                return successResults.ToDictionary(x => x.Key, x => x.Value);
            case StatusResultType.Warning:
                // a check is considered warn status if one check is warn and all others are success or info
                IEnumerable<KeyValuePair<string, IEnumerable<HealthCheckStatus>>> warnResults =
                    ResultsAsDictionary.Where(x =>
                        x.Value.Any(y => y.ResultType == StatusResultType.Warning) && x.Value.All(y =>
                            y.ResultType == StatusResultType.Warning || y.ResultType == StatusResultType.Success ||
                            y.ResultType == StatusResultType.Info));
                return warnResults.ToDictionary(x => x.Key, x => x.Value);
            case StatusResultType.Error:
                // a check is considered error status if any check is error
                IEnumerable<KeyValuePair<string, IEnumerable<HealthCheckStatus>>> errorResults =
                    ResultsAsDictionary.Where(x => x.Value.Any(y => y.ResultType == StatusResultType.Error));
                return errorResults.ToDictionary(x => x.Key, x => x.Value);
            case StatusResultType.Info:
                // a check is considered info status if all checks are info
                IEnumerable<KeyValuePair<string, IEnumerable<HealthCheckStatus>>> infoResults =
                    ResultsAsDictionary.Where(x => x.Value.All(y => y.ResultType == StatusResultType.Info));
                return infoResults.ToDictionary(x => x.Key, x => x.Value);
        }

        return null;
    }

    private string SimpleHtmlToMarkDown(string html) =>
        html.Replace("<strong>", "**")
            .Replace("</strong>", "**")
            .Replace("<em>", "*")
            .Replace("</em>", "*");
}
