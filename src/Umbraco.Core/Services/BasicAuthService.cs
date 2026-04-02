using System.Net;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Core.Services.Implement;

/// <summary>
/// Provides functionality for basic authentication operations including IP allow-listing and shared secret validation.
/// </summary>
public class BasicAuthService : IBasicAuthService
{
    private readonly IIpAddressUtilities _ipAddressUtilities;
    private BasicAuthSettings _basicAuthSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicAuthService"/> class.
    /// </summary>
    /// <param name="optionsMonitor">The options monitor for basic authentication settings.</param>
    /// <param name="ipAddressUtilities">The IP address utilities for checking allow-listed addresses.</param>
    public BasicAuthService(IOptionsMonitor<BasicAuthSettings> optionsMonitor, IIpAddressUtilities ipAddressUtilities)
    {
        _ipAddressUtilities = ipAddressUtilities;
        _basicAuthSettings = optionsMonitor.CurrentValue;

        optionsMonitor.OnChange(basicAuthSettings => _basicAuthSettings = basicAuthSettings);
    }

    /// <inheritdoc />
    public bool IsBasicAuthEnabled() => _basicAuthSettings.Enabled;

    /// <inheritdoc />
    public bool IsRedirectToLoginPageEnabled() => _basicAuthSettings.RedirectToLoginPage;

    /// <inheritdoc />
    public bool IsIpAllowListed(IPAddress clientIpAddress)
    {
        foreach (var allowedIpString in _basicAuthSettings.AllowedIPs)
        {
            if (_ipAddressUtilities.IsAllowListed(clientIpAddress, allowedIpString))
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    public bool HasCorrectSharedSecret(IDictionary<string, StringValues> headers)
    {
        var headerName = _basicAuthSettings.SharedSecret.HeaderName;
        var sharedSecret = _basicAuthSettings.SharedSecret.Value;

        if (string.IsNullOrWhiteSpace(headerName) || string.IsNullOrWhiteSpace(sharedSecret))
        {
            return false;
        }

        return headers.TryGetValue(headerName, out StringValues value) && value.Equals(sharedSecret);
    }
}
