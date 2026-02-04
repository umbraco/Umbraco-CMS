namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     A no-operation implementation of <see cref="IRequestCultureService"/> that does not track or return cultures.
/// </summary>
public sealed class NoopRequestCultureService : IRequestCultureService
{
    /// <inheritdoc />
    public string? GetRequestedCulture() => null;

    /// <summary>
    ///     Sets the request culture. This implementation does nothing.
    /// </summary>
    /// <param name="culture">The culture to set.</param>
    public void SetRequestCulture(string culture)
    {
    }
}
