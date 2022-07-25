using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

public class ExamineTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly IExamineIndexCountService _examineIndexCountService;

    public ExamineTelemetryProvider(IExamineIndexCountService examineIndexCountService) =>
        _examineIndexCountService = examineIndexCountService;

    public IEnumerable<UsageInformation> GetInformation()
    {
        var indexes = _examineIndexCountService.GetCount();
        yield return new UsageInformation(Constants.Telemetry.ExamineIndexCount, indexes);
    }
}
