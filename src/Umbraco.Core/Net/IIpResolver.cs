namespace Umbraco.Cms.Core.Net;

public interface IIpResolver
{
    string GetCurrentRequestIpAddress();
}
