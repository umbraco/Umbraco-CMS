using System.Net;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.Common.Mvc;

public class IpAddressUtilities : IIpAddressUtilities
{
    public bool IsAllowListed(IPAddress clientIpAddress, string allowedIpString)
    {
        var subnetmaskIndex = allowedIpString.LastIndexOf('/');
        if (subnetmaskIndex >= 0) // It's a network
        {
            if (IPNetwork.TryParse(allowedIpString, out IPNetwork allowedIp) && allowedIp.Contains(clientIpAddress))
            {
                return true;
            }

            return false;
        }

        // Assume ip address
        if (IPAddress.TryParse(allowedIpString, out IPAddress? allowedIpAddress) && allowedIpAddress.Equals(clientIpAddress))
        {
            return true;
        }

        return false;


    }
}
