namespace Umbraco.Cms.Core.Web;

/// <summary>
/// Defines session management operations.
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Gets the session value for the specified key.
    /// </summary>
    /// <param name="key">The session key.</param>
    /// <returns>The session value if found; otherwise, <c>null</c>.</returns>
    string? GetSessionValue(string key);

    /// <summary>
    /// Sets the session value for the specified key.
    /// </summary>
    /// <param name="key">The session key.</param>
    /// <param name="value">The value to store in the session.</param>
    void SetSessionValue(string key, string value);

    /// <summary>
    /// Clears the session value for the specified key.
    /// </summary>
    /// <param name="key">The session key to clear.</param>
    void ClearSessionValue(string key);
}
