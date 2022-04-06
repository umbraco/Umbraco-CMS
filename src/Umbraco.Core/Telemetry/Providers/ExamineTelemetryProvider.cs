using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Telemetry.Providers
{
    public class ExamineTelemetryProvider : IDetailedTelemetryProvider
    {
        private readonly IExamineIndexCountService _examineIndexCountService;

        public ExamineTelemetryProvider(IExamineIndexCountService examineIndexCountService)
        {
            _examineIndexCountService = examineIndexCountService;
        }

        public IEnumerable<UsageInformation> GetInformation()
        {
            var result = new List<UsageInformation>();
            var indexes = _examineIndexCountService.GetCount();

            result.Add(new UsageInformation("IndexCount", indexes));
            return result;
        }
    }
}
