using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.HealthChecks;

public class HealthCheckCollection : BuilderCollectionBase<HealthCheck>
{
    public HealthCheckCollection(Func<IEnumerable<HealthCheck>> items)
        : base(items)
    {
    }
}
