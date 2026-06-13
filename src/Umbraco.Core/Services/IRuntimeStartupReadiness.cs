namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Reports whether the runtime is ready to serve front-end requests.
/// </summary>
/// <remarks>
/// During an unattended upgrade the HTTP server accepts requests while the upgrade runs on a background thread.
/// The runtime level flips to <see cref="RuntimeLevel.Run"/> before component/application-starting initialization
/// (e.g. document URL routing) has completed. This signal lets the front end stay on the maintenance page until
/// that initialization is finished, rather than serving requests against not-yet-initialized services.
/// On a normal boot nothing toggles this, so it always reports ready.
/// </remarks>
public interface IRuntimeStartupReadiness
{
    /// <summary>
    /// Gets a value indicating whether the runtime is ready to serve front-end requests.
    /// Defaults to <c>true</c> so a normal boot is never gated.
    /// </summary>
    bool IsReadyToServe { get; }
}
