namespace Umbraco.Core.Configuration
{
    public interface IRuntimeSettings
    {
        int? MaxQueryStringLength { get; }
        int? MaxRequestLength { get; }
    }
}
