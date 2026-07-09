namespace Umbraco.Cms.Core.Net;

/// <summary>
/// Provides functionality to retrieve the user agent string from the current request.
/// </summary>
public interface IUserAgentProvider
{
    /// <summary>
    /// Gets the user agent string from the current HTTP request.
    /// </summary>
    /// <returns>The user agent string, or <c>null</c> if not available.</returns>
    string? GetUserAgent();
}
