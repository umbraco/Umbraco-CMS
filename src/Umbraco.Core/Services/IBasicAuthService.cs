using System.Net;

namespace Umbraco.Cms.Core.Services;

public interface IBasicAuthService
{
    bool IsBasicAuthEnabled();
    bool IsIpAllowListed(IPAddress clientIpAddress);
}
