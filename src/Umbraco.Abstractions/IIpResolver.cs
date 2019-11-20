namespace Umbraco.Core
{
    public interface IIpResolver
    {
        string GetCurrentRequestIpAddress();
    }
}
