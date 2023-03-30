using Umbraco.Cms.Core.Models.ContentApi;

namespace Umbraco.Cms.Core.ContentApi;

public sealed class NoopRequestRedirectService : IRequestRedirectService
{
    /// <inheritdoc />
    public IApiContentRoute? GetRedirectRoute(string requestedPath) => null;
}
