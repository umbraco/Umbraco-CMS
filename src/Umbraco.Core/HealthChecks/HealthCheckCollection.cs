using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Core.HealthChecks
{
    public class HealthCheckCollection : BuilderCollectionBase<HealthCheck>
    {
        public HealthCheckCollection(IEnumerable<HealthCheck> items)
            : base(items)
        { }
    }
}
