namespace Umbraco.Core.Net
{
    public interface IIpResolver
    {
        string GetCurrentRequestIpAddress();
    }
}
