// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Search.Provider.Examine.Telemetry;

/// <summary>
/// Provides telemetry data and metrics related to Examine indexing and search operations within Umbraco.
/// </summary>
internal sealed class ExamineTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly IExamineManager _examineManager;

    public ExamineTelemetryProvider(IExamineManager examineManager) => _examineManager = examineManager;

    /// <summary>
    /// Retrieves telemetry usage information about the Examine indexes in the system.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{UsageInformation}"/> containing usage data for Examine indexes.</returns>
    public IEnumerable<UsageInformation> GetInformation()
    {
        yield return new UsageInformation(Umbraco.Cms.Core.Constants.Telemetry.ExamineIndexCount, _examineManager.Indexes.Count());
    }
}
