using System.Net;
using Microsoft.Extensions.Primitives;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides services for basic authentication operations.
/// </summary>
public interface IBasicAuthService
{
    /// <summary>
    ///     Gets a value indicating whether basic authentication is enabled.
    /// </summary>
    /// <returns><c>true</c> if basic authentication is enabled; otherwise, <c>false</c>.</returns>
    bool IsBasicAuthEnabled();

    /// <summary>
    ///     Checks if the specified IP address is in the allow list.
    /// </summary>
    /// <param name="clientIpAddress">The client IP address to check.</param>
    /// <returns><c>true</c> if the IP address is allow-listed; otherwise, <c>false</c>.</returns>
    bool IsIpAllowListed(IPAddress clientIpAddress);

    /// <summary>
    ///     Checks if the request headers contain the correct shared secret.
    /// </summary>
    /// <param name="headers">The HTTP request headers.</param>
    /// <returns><c>true</c> if the headers contain the correct shared secret; otherwise, <c>false</c>.</returns>
    bool HasCorrectSharedSecret(IDictionary<string, StringValues> headers) => false;

    /// <summary>
    ///     Gets a value indicating whether redirect to login page is enabled for unauthenticated requests.
    /// </summary>
    /// <returns><c>true</c> if redirect to login page is enabled; otherwise, <c>false</c>.</returns>
    bool IsRedirectToLoginPageEnabled() => false;
}
