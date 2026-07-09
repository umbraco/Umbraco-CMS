using Umbraco.Cms.Core.Models.DeliveryApi;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     A no-operation implementation of <see cref="IRequestRedirectService"/> that never returns redirects.
/// </summary>
public sealed class NoopRequestRedirectService : IRequestRedirectService
{
    /// <inheritdoc />
    public IApiContentRoute? GetRedirectRoute(string requestedPath) => null;
}
