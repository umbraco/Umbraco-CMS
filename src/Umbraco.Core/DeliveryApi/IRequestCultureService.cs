namespace Umbraco.Cms.Core.DeliveryApi;

public interface IRequestCultureService
{
    /// <summary>
    ///     Gets the requested culture from the "Accept-Language" header, if present.
    /// </summary>
    string? GetRequestedCulture();
}
