using Umbraco.Cms.Core.Models.DeliveryApi;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class NoopRequestRedirectService : IRequestRedirectService
{
    /// <inheritdoc />
    public IApiContentRoute? GetRedirectRoute(string requestedPath) => null;
}
