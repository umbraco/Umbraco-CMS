namespace Umbraco.Abstractions
{
    public interface IRuntimeSettings
    {
        int? MaxQueryStringLength { get; }
        int? MaxRequestLength { get; }
    }
}
