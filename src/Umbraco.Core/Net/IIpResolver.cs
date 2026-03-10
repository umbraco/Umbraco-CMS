namespace Umbraco.Cms.Core.Net;

/// <summary>
/// Provides functionality to resolve the IP address of the current request.
/// </summary>
public interface IIpResolver
{
    /// <summary>
    /// Gets the IP address of the current HTTP request.
    /// </summary>
    /// <returns>The IP address as a string.</returns>
    string GetCurrentRequestIpAddress();
}
