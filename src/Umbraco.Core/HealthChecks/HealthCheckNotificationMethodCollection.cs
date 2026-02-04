using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.HealthChecks.NotificationMethods;

namespace Umbraco.Cms.Core.HealthChecks;

/// <summary>
///     Represents a collection of <see cref="IHealthCheckNotificationMethod" /> instances.
/// </summary>
public class HealthCheckNotificationMethodCollection : BuilderCollectionBase<IHealthCheckNotificationMethod>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="HealthCheckNotificationMethodCollection" /> class.
    /// </summary>
    /// <param name="items">A factory function that returns the collection items.</param>
    public HealthCheckNotificationMethodCollection(Func<IEnumerable<IHealthCheckNotificationMethod>> items)
        : base(items)
    {
    }
}
