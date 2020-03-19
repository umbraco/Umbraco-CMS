namespace Umbraco.Core.Configuration
{
    public interface IImagingSettings
    {
        int MaxBrowserCacheDays { get;}
        int MaxCacheDays { get; }
        uint CachedNameLength { get; }
        int MaxResizeWidth { get; }
        int MaxResizeHeight { get; }
        string CacheFolder { get; }
    }
}
