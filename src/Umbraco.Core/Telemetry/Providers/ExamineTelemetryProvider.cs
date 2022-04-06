using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Telemetry.Providers
{
    public class ExamineTelemetryProvider : IDetailedTelemetryProvider
    {
        private readonly IExamineIndexCountService _examineIndexCountService;

        public ExamineTelemetryProvider(IExamineIndexCountService examineIndexCountService) => _examineIndexCountService = examineIndexCountService;

        public IEnumerable<UsageInformation> GetInformation()
        {
            var indexes = _examineIndexCountService.GetCount();
            yield return new UsageInformation("IndexCount", indexes);
        }
    }
}
