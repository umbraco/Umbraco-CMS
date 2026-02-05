namespace Umbraco.Cms.Core.Web;

/// <summary>
/// Provides access to HTTP request values.
/// </summary>
public interface IRequestAccessor
{
    /// <summary>
    /// Gets the request value for the specified name from request, form, or query string.
    /// </summary>
    /// <param name="name">The name of the value to retrieve.</param>
    /// <returns>The request value if found; otherwise, <c>null</c>.</returns>
    string? GetRequestValue(string name);

    /// <summary>
    /// Gets the query string value for the specified name.
    /// </summary>
    /// <param name="name">The name of the query string parameter.</param>
    /// <returns>The query string value if found; otherwise, <c>null</c>.</returns>
    string? GetQueryStringValue(string name);

    /// <summary>
    /// Gets the current request URL.
    /// </summary>
    /// <returns>The current request <see cref="Uri"/> if available; otherwise, <c>null</c>.</returns>
    Uri? GetRequestUrl();
}
