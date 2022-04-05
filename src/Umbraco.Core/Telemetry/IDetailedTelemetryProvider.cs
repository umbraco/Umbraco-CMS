using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services
{
    public interface IDetailedTelemetryProvider
    {
        public IEnumerable<UsageInformation> GetInformation();
    }
}
