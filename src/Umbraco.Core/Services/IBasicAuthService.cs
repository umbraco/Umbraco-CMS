using System.Net;
using Microsoft.Extensions.Primitives;

namespace Umbraco.Cms.Core.Services;

public interface IBasicAuthService
{
    bool IsBasicAuthEnabled();
    bool IsIpAllowListed(IPAddress clientIpAddress);
    bool HasCorrectSharedSecret(IDictionary<string, StringValues> headers) => false;

    bool IsRedirectToLoginPageEnabled() => false;
}
