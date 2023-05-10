using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     A handler that appends field values to the Delivery API content index.
/// </summary>
public interface IContentIndexHandler : IDiscoverable
{
    /// <summary>
    ///     Calculates the field values for a given content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <returns>The values to add to the index.</returns>
    IEnumerable<IndexFieldValue> GetFieldValues(IContent content);

    /// <summary>
    ///     Returns the field definitions required to support the field values in the index.
    /// </summary>
    /// <returns>The field definitions.</returns>
    IEnumerable<IndexField> GetFields();
}
