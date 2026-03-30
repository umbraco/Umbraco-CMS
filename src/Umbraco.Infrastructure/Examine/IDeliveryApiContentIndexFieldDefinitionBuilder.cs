using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
/// Provides a contract for building field definitions used in the Delivery API content index.
/// </summary>
public interface IDeliveryApiContentIndexFieldDefinitionBuilder
{
    /// <summary>
    /// Builds a collection of field definitions for the Delivery API content index.
    /// </summary>
    /// <returns>A <see cref="FieldDefinitionCollection"/> containing the field definitions for the index.</returns>
    FieldDefinitionCollection Build();
}
