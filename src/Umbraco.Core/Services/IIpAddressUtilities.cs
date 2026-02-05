using System.Net;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides utility methods for working with IP addresses.
/// </summary>
public interface IIpAddressUtilities
{
    /// <summary>
    ///     Determines whether the specified client IP address is in the allow list.
    /// </summary>
    /// <param name="clientIpAddress">The client IP address to check.</param>
    /// <param name="allowedIpString">The allowed IP string pattern to match against.</param>
    /// <returns><c>true</c> if the IP address is in the allow list; otherwise, <c>false</c>.</returns>
    bool IsAllowListed(IPAddress clientIpAddress, string allowedIpString);
}
