using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
/// Represents a contract for building value sets used in the Delivery API content index.
/// </summary>
public interface IDeliveryApiContentIndexValueSetBuilder : IValueSetBuilder<IContent>
{
}
