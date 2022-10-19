using System.Net;

namespace Umbraco.Cms.Core.Services;

public interface IIpAddressUtilities
{
    bool IsAllowListed(IPAddress clientIpAddress, string allowedIpString);
}
