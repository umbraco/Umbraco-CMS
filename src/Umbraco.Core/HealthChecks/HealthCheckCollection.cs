using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.HealthChecks;

/// <summary>
///     Represents a collection of <see cref="HealthCheck" /> instances.
/// </summary>
public class HealthCheckCollection : BuilderCollectionBase<HealthCheck>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="HealthCheckCollection" /> class.
    /// </summary>
    /// <param name="items">A factory function that returns the collection items.</param>
    public HealthCheckCollection(Func<IEnumerable<HealthCheck>> items)
        : base(items)
    {
    }
}
