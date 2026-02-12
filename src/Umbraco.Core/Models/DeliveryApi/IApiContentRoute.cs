namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents a route to content in the Delivery API.
/// </summary>
public interface IApiContentRoute
{
    /// <summary>
    ///     Gets the URL path of the content.
    /// </summary>
    string Path { get; }

    /// <summary>
    ///     Gets or sets the query string associated with the route.
    /// </summary>
    public string? QueryString
    {
        get => null; set { }
    }

    /// <summary>
    ///     Gets the start item for this route.
    /// </summary>
    IApiContentStartItem StartItem { get; }
}
