namespace Umbraco.Cms.Core.DeliveryApi;

public interface IRequestCultureService
{
    /// <summary>
    ///     Gets the requested culture from the "Accept-Language" header, if present.
    /// </summary>
    string? GetRequestedCulture();

    [Obsolete("Use IVariationContextAccessor to manipulate the variation context. Scheduled for removal in V17.")]
    void SetRequestCulture(string culture);
}
