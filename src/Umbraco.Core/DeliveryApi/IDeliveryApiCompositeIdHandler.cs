namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a handler for creating and decomposing composite IDs used in the Delivery API index.
/// </summary>
public interface IDeliveryApiCompositeIdHandler
{
    /// <summary>
    ///     Creates a composite index ID from the specified content ID and culture.
    /// </summary>
    /// <param name="id">The content ID.</param>
    /// <param name="culture">The culture code.</param>
    /// <returns>A composite index ID string.</returns>
    string IndexId(int id, string culture);

    /// <summary>
    ///     Decomposes a composite index ID into its component parts.
    /// </summary>
    /// <param name="indexId">The composite index ID to decompose.</param>
    /// <returns>A <see cref="DeliveryApiIndexCompositeIdModel"/> containing the decomposed ID and culture.</returns>
    DeliveryApiIndexCompositeIdModel Decompose(string indexId);
}
