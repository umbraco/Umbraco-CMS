namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents a start item for content routing in the Delivery API.
/// </summary>
public interface IApiContentStartItem
{
    /// <summary>
    ///     Gets the unique identifier of the start item.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    ///     Gets the path of the start item.
    /// </summary>
    string Path { get; }
}
