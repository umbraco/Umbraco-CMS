namespace Umbraco.Cms.Api.Management.Routing;

/// <summary>
/// Represents a service that constructs absolute URLs from relative paths or route information.
/// </summary>
public interface IAbsoluteUrlBuilder
{
    /// <summary>
    /// Converts a relative or partial URL to an absolute <see cref="Uri"/>.
    /// </summary>
    /// <param name="url">The URL to convert to an absolute URL.</param>
    /// <returns>An absolute <see cref="Uri"/> representing the provided URL.</returns>
    Uri ToAbsoluteUrl(string url);
}
