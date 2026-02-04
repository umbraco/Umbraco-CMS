namespace Umbraco.Cms.Core.Net;

/// <summary>
/// Provides functionality to resolve the session identifier for the current request.
/// </summary>
public interface ISessionIdResolver
{
    /// <summary>
    /// Gets the session identifier for the current request, if available.
    /// </summary>
    string? SessionId { get; }
}
