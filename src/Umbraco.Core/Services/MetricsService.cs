using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services
{
    public class MetricsService : IMetricsService
    {
        public MetricsService()
        {

        }
        public IEnumerable<Metric> GetMetrics() => throw new System.NotImplementedException();
    }
}
