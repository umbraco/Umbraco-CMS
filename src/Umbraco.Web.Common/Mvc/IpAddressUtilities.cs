using System.Net;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.Common.Mvc;

extern alias IPNetwork2;

public class IpAddressUtilities : IIpAddressUtilities
{
    public bool IsAllowListed(IPAddress clientIpAddress, string allowedIpString)
    {
        if (IPNetwork2.System.Net.IPNetwork.TryParse(allowedIpString, out IPNetwork2.System.Net.IPNetwork allowedIp) && allowedIp.Contains(clientIpAddress))
        {
            return true;
        }

        return false;
    }
}
