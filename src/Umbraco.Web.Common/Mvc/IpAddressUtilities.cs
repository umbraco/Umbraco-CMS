using System.Net;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.Common.Mvc;

public class IpAddressUtilities : IIpAddressUtilities
{
    public bool IsAllowListed(IPAddress clientIpAddress, string allowedIpString)
    {
        if (IPNetwork.TryParse(allowedIpString, out IPNetwork allowedIp) && allowedIp.Contains(clientIpAddress))
        {
            return true;
        }

        return false;
    }
}
