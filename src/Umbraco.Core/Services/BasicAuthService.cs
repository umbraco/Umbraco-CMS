using System.Net;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Core.Services.Implement;

public class BasicAuthService : IBasicAuthService
{
    private readonly IIpAddressUtilities _ipAddressUtilities;
    private BasicAuthSettings _basicAuthSettings;

    public BasicAuthService(IOptionsMonitor<BasicAuthSettings> optionsMonitor, IIpAddressUtilities ipAddressUtilities)
    {
        _ipAddressUtilities = ipAddressUtilities;
        _basicAuthSettings = optionsMonitor.CurrentValue;

        optionsMonitor.OnChange(basicAuthSettings => _basicAuthSettings = basicAuthSettings);
    }

    public bool IsBasicAuthEnabled() => _basicAuthSettings.Enabled;
    public bool IsRedirectToLoginPageEnabled() => _basicAuthSettings.RedirectToLoginPage;

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
