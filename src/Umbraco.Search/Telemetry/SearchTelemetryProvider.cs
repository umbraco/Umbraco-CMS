using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;
using Umbraco.Search.Services;

namespace Umbraco.Search.Telemetry;

public class SearchTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly IIndexCountService _examineIndexCountService;

    public SearchTelemetryProvider(IIndexCountService examineIndexCountService) =>
        _examineIndexCountService = examineIndexCountService;

    public IEnumerable<UsageInformation> GetInformation()
    {
        var indexes = _examineIndexCountService.GetCount();
        yield return new UsageInformation(Cms.Core.Constants.Telemetry.ExamineIndexCount, indexes);
    }
}
