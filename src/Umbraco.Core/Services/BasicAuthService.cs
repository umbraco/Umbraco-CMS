using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.Services.Implement;

public class BasicAuthService : IBasicAuthService
{
    private readonly IIpAddressUtilities _ipAddressUtilities;
    private BasicAuthSettings _basicAuthSettings;

    // Scheduled for removal in v12
    [Obsolete("Please use the contructor that takes an IIpadressUtilities instead")]
    public BasicAuthService(IOptionsMonitor<BasicAuthSettings> optionsMonitor)
        : this(optionsMonitor, StaticServiceProvider.Instance.GetRequiredService<IIpAddressUtilities>())
    {
        _basicAuthSettings = optionsMonitor.CurrentValue;

        optionsMonitor.OnChange(basicAuthSettings => _basicAuthSettings = basicAuthSettings);
    }

    public BasicAuthService(IOptionsMonitor<BasicAuthSettings> optionsMonitor, IIpAddressUtilities ipAddressUtilities)
    {
        _ipAddressUtilities = ipAddressUtilities;
        _basicAuthSettings = optionsMonitor.CurrentValue;

        optionsMonitor.OnChange(basicAuthSettings => _basicAuthSettings = basicAuthSettings);
    }

    public bool IsBasicAuthEnabled() => _basicAuthSettings.Enabled;

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
}
