namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class NoopRequestCultureService : IRequestCultureService
{
    /// <inheritdoc />
    public string? GetRequestedCulture() => null;

    /// <inheritdoc />
    public void SetRequestCulture(string culture)
    {
    }
}
