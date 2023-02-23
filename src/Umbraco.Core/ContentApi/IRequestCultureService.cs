namespace Umbraco.Cms.Core.ContentApi;

public interface IRequestCultureService
{
    /// <summary>
    ///     Gets the requested culture from the "Accept-Language" header, if present.
    /// </summary>
    string? GetRequestedCulture();

    /// <summary>
    ///     Updates the current request culture if applicable.
    /// </summary>
    /// <param name="culture">The culture to use for the current request.</param>
    void SetRequestCulture(string culture);
}
