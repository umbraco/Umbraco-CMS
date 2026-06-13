using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Install;

/// <summary>
/// Internal control surface for toggling <see cref="IRuntimeStartupReadiness.IsReadyToServe"/>.
/// Kept out of the public read interface so only the runtime (this assembly) can change readiness.
/// </summary>
internal interface IRuntimeStartupReadinessControl
{
    /// <summary>
    /// Marks the runtime as not yet ready to serve front-end requests (background finalization in progress).
    /// </summary>
    void SetNotReady();

    /// <summary>
    /// Marks the runtime as ready to serve front-end requests.
    /// </summary>
    void SetReady();
}
