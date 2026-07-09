using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

/// <summary>
/// Provides telemetry data and metrics related to Examine indexing and search operations within Umbraco.
/// </summary>
public class ExamineTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly IExamineIndexCountService _examineIndexCountService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExamineTelemetryProvider"/> class.
    /// </summary>
    /// <param name="examineIndexCountService">An instance of <see cref="IExamineIndexCountService"/> used to provide information about the number of Examine indexes.</param>
    public ExamineTelemetryProvider(IExamineIndexCountService examineIndexCountService) =>
        _examineIndexCountService = examineIndexCountService;

    /// <summary>
    /// Retrieves telemetry usage information about the Examine indexes in the system.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{UsageInformation}"/> containing usage data for Examine indexes.</returns>
    public IEnumerable<UsageInformation> GetInformation()
    {
        var indexes = _examineIndexCountService.GetCount();
        yield return new UsageInformation(Constants.Telemetry.ExamineIndexCount, indexes);
    }
}
