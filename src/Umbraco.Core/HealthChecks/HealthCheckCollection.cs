using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.HealthChecks
{
    public class HealthCheckCollection : BuilderCollectionBase<HealthCheck>
    {
        public HealthCheckCollection(IEnumerable<HealthCheck> items)
            : base(items)
        { }
    }
}
