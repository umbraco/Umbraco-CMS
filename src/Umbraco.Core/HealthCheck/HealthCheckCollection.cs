using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Core.HealthCheck
{
    public class HealthCheckCollection : BuilderCollectionBase<Core.HealthCheck.HealthCheck>
    {
        public HealthCheckCollection(IEnumerable<Core.HealthCheck.HealthCheck> items)
            : base(items)
        { }
    }
}
