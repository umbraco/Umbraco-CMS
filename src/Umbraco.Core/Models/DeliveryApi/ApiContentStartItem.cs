namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents a start item for content routing in the Delivery API.
/// </summary>
public sealed class ApiContentStartItem : IApiContentStartItem
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiContentStartItem" /> class.
    /// </summary>
    /// <param name="id">The unique identifier of the start item.</param>
    /// <param name="path">The path of the start item.</param>
    public ApiContentStartItem(Guid id, string path)
    {
        Id = id;
        Path = path;
    }

    /// <inheritdoc />
    public Guid Id { get; }

    /// <inheritdoc />
    public string Path { get; }
}
