using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Infrastructure.Runtime;

/// <inheritdoc />
internal sealed class ContentRoutingReadiness : IContentRoutingReadiness
{
    private volatile bool _isReady;

    /// <inheritdoc />
    public bool IsReady => _isReady;

    /// <inheritdoc />
    public void MarkReady() => _isReady = true;
}
