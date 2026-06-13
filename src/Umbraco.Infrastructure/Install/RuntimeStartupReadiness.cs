using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Install;

/// <inheritdoc cref="IRuntimeStartupReadiness" />
internal sealed class RuntimeStartupReadiness : IRuntimeStartupReadiness, IRuntimeStartupReadinessControl
{
    // Single writer (the unattended upgrade background thread), many readers (request threads).
    // Only visibility is required, not atomicity of a compound operation, so volatile is sufficient.
    private volatile bool _isReady = true; // Default ready: a normal boot never touches this.

    /// <inheritdoc />
    public bool IsReadyToServe => _isReady;

    /// <inheritdoc />
    public void SetNotReady() => _isReady = false;

    /// <inheritdoc />
    public void SetReady() => _isReady = true;
}
