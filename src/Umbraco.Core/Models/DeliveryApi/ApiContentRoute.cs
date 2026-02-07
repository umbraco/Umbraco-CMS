namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents a route to content in the Delivery API.
/// </summary>
public sealed class ApiContentRoute : IApiContentRoute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiContentRoute" /> class.
    /// </summary>
    /// <param name="path">The URL path of the content.</param>
    /// <param name="startItem">The start item for this route.</param>
    public ApiContentRoute(string path, ApiContentStartItem startItem)
    {
        Path = path;
        StartItem = startItem;
    }

    /// <inheritdoc />
    public string Path { get; }

    /// <inheritdoc />
    public string? QueryString { get; set; }

    /// <inheritdoc />
    public IApiContentStartItem StartItem { get; }
}
