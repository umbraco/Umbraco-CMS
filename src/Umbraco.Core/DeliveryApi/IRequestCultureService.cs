namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a service that retrieves the requested culture from the current HTTP request.
/// </summary>
public interface IRequestCultureService
{
    /// <summary>
    ///     Gets the requested culture from the "Accept-Language" header, if present.
    /// </summary>
    string? GetRequestedCulture();
}
