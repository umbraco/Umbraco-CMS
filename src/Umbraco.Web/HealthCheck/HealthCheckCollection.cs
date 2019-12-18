using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Web.HealthCheck
{
    public class HealthCheckCollection : BuilderCollectionBase<HealthCheck>
    {
        public HealthCheckCollection(IEnumerable<HealthCheck> items)
            : base(items)
        { }
    }
}
